using mini_mes_be.DTOs.Auth;

namespace mini_mes_be.Services;

public interface IAuthService
{
    Task<TokenResponse> LoginAsync(LoginRequest request, string ipAddress);
    Task<TokenResponse> RegisterAsync(RegisterRequest request, string ipAddress);
    Task<TokenResponse> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task RevokeTokenAsync(string refreshToken, string ipAddress);
}
