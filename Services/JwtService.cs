using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using mini_mes_be.Models;

namespace mini_mes_be.Services;

public class JwtService : IJwtService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpiryMinutes;

    public JwtService(IConfiguration config)
    {
        _secretKey = config["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");
        _issuer = config["Jwt:Issuer"] ?? "mini-mes-api";
        _audience = config["Jwt:Audience"] ?? "mini-mes-client";
        _accessTokenExpiryMinutes = config.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 60);
    }

    /// <summary>Generates a signed JWT access token for the given user.</summary>
    public string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.username),
            new Claim(JwtRegisteredClaimNames.Email, user.email),
            new Claim(ClaimTypes.Role, user.role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>Generates a cryptographically secure opaque refresh token.</summary>
    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Validates the access token and returns the user's integer ID (Sub claim) if valid; null otherwise.
    /// </summary>
    public int? ValidateAccessToken(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var handler = new JwtSecurityTokenHandler();

        try
        {
            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            var jwt = (JwtSecurityToken)validatedToken;
            var sub = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            return int.TryParse(sub, out var userId) ? userId : null;
        }
        catch
        {
            return null;
        }
    }
}
