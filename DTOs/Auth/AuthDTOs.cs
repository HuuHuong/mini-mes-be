namespace mini_mes_be.DTOs.Auth;

/// <summary>Login using email + password.</summary>
public record LoginRequest(string email, string password);

/// <summary>Register a new account. Email must be unique.</summary>
public record RegisterRequest(string username, string email, string password);

public record RefreshTokenRequest(string refresh_token);

public record TokenResponse(
    string access_token,
    string refresh_token,
    /// <summary>Unix timestamp (seconds) when the access token expires.</summary>
    long access_token_expiry,
    int user_id,
    string username,
    string role
);

public record UserMeResponse(
    int? id,
    string? username,
    string? email,
    string? role
);
