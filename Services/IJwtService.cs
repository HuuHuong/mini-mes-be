using mini_mes_be.Models;

namespace mini_mes_be.Services;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();

    /// <summary>
    /// Validates the access token and returns the user's integer ID if valid; null otherwise.
    /// </summary>
    int? ValidateAccessToken(string token);
}
