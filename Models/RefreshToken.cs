namespace mini_mes_be.Models;

/// <summary>
/// Refresh token entity persisted per user session.
/// All timestamps are Unix time (seconds since Unix epoch, UTC).
/// </summary>
public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;

    /// <summary>Unix timestamp (seconds) when this token expires.</summary>
    public long ExpiresAt { get; set; }

    public bool IsRevoked { get; set; } = false;
    public string? ReplacedByToken { get; set; }
    public string? CreatedByIp { get; set; }
    public string? RevokedByIp { get; set; }

    /// <summary>Unix timestamp (seconds) when this token was revoked. Null if still active.</summary>
    public long? RevokedAt { get; set; }

    // FK — int to match User.Id
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public bool IsActive => !IsRevoked && DateTimeOffset.UtcNow.ToUnixTimeSeconds() < ExpiresAt;
}
