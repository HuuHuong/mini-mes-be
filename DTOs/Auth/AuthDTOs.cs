namespace mini_mes_be.DTOs.Auth;

/// <summary>Login using email + password.</summary>
public record LoginRequest(string Email, string Password);

/// <summary>Register a new account. Email must be unique.</summary>
public record RegisterRequest(string Username, string Email, string Password);

public record RefreshTokenRequest(string RefreshToken);

public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    /// <summary>Unix timestamp (seconds) when the access token expires.</summary>
    long AccessTokenExpiry,
    int UserId,
    string Username,
    string Role
);
