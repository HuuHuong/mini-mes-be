using Microsoft.EntityFrameworkCore;
using mini_mes_be.Data;
using mini_mes_be.DTOs.Auth;
using mini_mes_be.Models;

namespace mini_mes_be.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtService _jwt;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IJwtService jwt, IConfiguration config)
    {
        _db = db;
        _jwt = jwt;
        _config = config;
    }

    // ── Register ──────────────────────────────────────────────────────────────

    public async Task<TokenResponse> RegisterAsync(RegisterRequest request, string ipAddress)
    {
        // Check duplicate email
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Email is already taken.");

        // Check duplicate username
        if (await _db.Users.AnyAsync(u => u.Username == request.Username))
            throw new InvalidOperationException("Username is already taken.");

        var user = new User
        {
            Username = request.Username,
            Email    = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "User"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return await IssueTokenPairAsync(user, ipAddress);
    }

    // ── Login (by Email + Password) ───────────────────────────────────────────

    public async Task<TokenResponse> LoginAsync(LoginRequest request, string ipAddress)
    {
        var user = await _db.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        return await IssueTokenPairAsync(user, ipAddress);
    }

    // ── Refresh ───────────────────────────────────────────────────────────────

    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        var stored = await _db.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.RefreshTokens)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (stored is null || !stored.IsActive)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        // Rotate: revoke old, issue new
        RevokeToken(stored, ipAddress, "Rotated");
        await _db.SaveChangesAsync();

        return await IssueTokenPairAsync(stored.User, ipAddress);
    }

    // ── Revoke ────────────────────────────────────────────────────────────────

    public async Task RevokeTokenAsync(string refreshToken, string ipAddress)
    {
        var stored = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
        if (stored is null || !stored.IsActive)
            throw new UnauthorizedAccessException("Invalid or already revoked refresh token.");

        RevokeToken(stored, ipAddress, "Revoked by user");
        await _db.SaveChangesAsync();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<TokenResponse> IssueTokenPairAsync(User user, string ipAddress)
    {
        var accessToken       = _jwt.GenerateAccessToken(user);
        var refreshTokenValue = _jwt.GenerateRefreshToken();
        var expiryDays        = _config.GetValue<int>("Jwt:RefreshTokenExpiryDays", 7);
        var now               = DateTimeOffset.UtcNow;

        var refreshTokenEntity = new RefreshToken
        {
            Token        = refreshTokenValue,
            ExpiresAt    = now.AddDays(expiryDays).ToUnixTimeSeconds(),
            CreatedByIp  = ipAddress,
            UserId       = user.Id
        };

        _db.RefreshTokens.Add(refreshTokenEntity);

        // Auto-purge stale tokens (keep DB clean)
        RemoveOldRefreshTokens(user);

        await _db.SaveChangesAsync();

        var accessExpiry = now.AddMinutes(
            _config.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 60)).ToUnixTimeSeconds();

        return new TokenResponse(accessToken, refreshTokenValue, accessExpiry, user.Id, user.Username, user.Role);
    }

    private static void RevokeToken(RefreshToken token, string ipAddress, string reason)
    {
        token.IsRevoked       = true;
        token.RevokedAt       = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        token.RevokedByIp     = ipAddress;
        token.ReplacedByToken = reason;
    }

    private static void RemoveOldRefreshTokens(User user)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeSeconds();
        var stale  = user.RefreshTokens
            .Where(rt => !rt.IsActive && rt.CreatedAt < cutoff)
            .ToList();

        foreach (var token in stale)
            user.RefreshTokens.Remove(token);
    }
}
