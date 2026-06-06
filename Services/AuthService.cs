using Microsoft.EntityFrameworkCore;
using mini_mes_be.Constants;
using mini_mes_be.Data;
using mini_mes_be.DTOs.Auth;
using mini_mes_be.Models;
using mini_mes_be.Middlewares;

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
        if (await _db.Users.AnyAsync(u => u.email == request.email))
            throw new InvalidOperationException(ErrorMessages.Auth.EmailAlreadyTaken);

        // Check duplicate username
        if (await _db.Users.AnyAsync(u => u.username == request.username))
            throw new InvalidOperationException(ErrorMessages.Auth.UsernameAlreadyTaken);

        var user = new User
        {
            username      = request.username,
            email         = request.email,
            password_hash = BCrypt.Net.BCrypt.HashPassword(request.password),
            role          = "User"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return await IssueTokenPairAsync(user, ipAddress);
    }

    // ── Login (by Email + Password) ───────────────────────────────────────────

    public async Task<TokenResponse> LoginAsync(LoginRequest request, string ipAddress)
    {
        var user = await _db.Users
            .Include(u => u.refresh_tokens)
            .FirstOrDefaultAsync(u => u.email == request.email);

        if (user is null)
            throw new AppValidationException(ErrorMessages.Auth.InvalidCredentials, "email", ErrorMessages.Auth.EmailNotRegistered, System.Net.HttpStatusCode.Unauthorized);

        if (!user.is_active)
            throw new AppValidationException(ErrorMessages.Auth.InvalidCredentials, "email", ErrorMessages.Auth.AccountInactive, System.Net.HttpStatusCode.Unauthorized);

        if (!BCrypt.Net.BCrypt.Verify(request.password, user.password_hash))
            throw new AppValidationException(ErrorMessages.Auth.InvalidCredentials, "password", ErrorMessages.Auth.IncorrectPassword, System.Net.HttpStatusCode.Unauthorized);

        return await IssueTokenPairAsync(user, ipAddress);
    }

    // ── Refresh ───────────────────────────────────────────────────────────────

    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        var stored = await _db.RefreshTokens
            .Include(rt => rt.user)
            .ThenInclude(u => u.refresh_tokens)
            .FirstOrDefaultAsync(rt => rt.token == refreshToken);

        if (stored is null || !stored.is_active)
            throw new UnauthorizedAccessException(ErrorMessages.Auth.InvalidRefreshToken);

        // Rotate: revoke old, issue new
        RevokeToken(stored, ipAddress, "Rotated");
        await _db.SaveChangesAsync();

        return await IssueTokenPairAsync(stored.user, ipAddress);
    }

    // ── Revoke ────────────────────────────────────────────────────────────────

    public async Task RevokeTokenAsync(string refreshToken, string ipAddress)
    {
        var stored = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.token == refreshToken);
        if (stored is null || !stored.is_active)
            throw new UnauthorizedAccessException(ErrorMessages.Auth.RevokedRefreshToken);

        RevokeToken(stored, ipAddress, "Revoked by user");
        await _db.SaveChangesAsync();
    }

    // ── Reset Password ─────────────────────────────────────────────────────

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _db.Users
            .Include(u => u.refresh_tokens)
            .FirstOrDefaultAsync(u => u.email == request.email);

        if (user is null)
            throw new AppValidationException(ErrorMessages.Auth.EmailNotRegistered, "email", ErrorMessages.Auth.EmailNotRegistered);

        if (!user.is_active)
            throw new AppValidationException(ErrorMessages.Auth.AccountInactive, "email", ErrorMessages.Auth.AccountInactive);

        if (request.new_password != request.confirm_password)
            throw new AppValidationException("Passwords do not match", "confirm_password", "Passwords do not match");

        user.password_hash = BCrypt.Net.BCrypt.HashPassword(request.new_password);

        // Revoke all active refresh tokens for this user so existing sessions are invalidated
        foreach (var token in user.refresh_tokens.Where(rt => rt.is_active))
            RevokeToken(token, "system", "Password reset");

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
            token         = refreshTokenValue,
            expires_at    = now.AddDays(expiryDays).ToUnixTimeSeconds(),
            created_by_ip = ipAddress,
            user_id       = user.id
        };

        _db.RefreshTokens.Add(refreshTokenEntity);

        // Auto-purge stale tokens (keep DB clean)
        RemoveOldRefreshTokens(user);

        await _db.SaveChangesAsync();

        var accessExpiry = now.AddMinutes(
            _config.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 60)).ToUnixTimeSeconds();

        return new TokenResponse(accessToken, refreshTokenValue, accessExpiry, user.id, user.username, user.role);
    }

    private static void RevokeToken(RefreshToken token, string ipAddress, string reason)
    {
        token.is_revoked       = true;
        token.revoked_at       = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        token.revoked_by_ip    = ipAddress;
        token.replaced_by_token = reason;
    }

    private static void RemoveOldRefreshTokens(User user)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeSeconds();
        var stale  = user.refresh_tokens
            .Where(rt => !rt.is_active && rt.created_at < cutoff)
            .ToList();

        foreach (var token in stale)
            user.refresh_tokens.Remove(token);
    }
}
