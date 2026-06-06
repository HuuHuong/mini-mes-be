namespace mini_mes_be.Models;

/// <summary>
/// Refresh token entity persisted per user session.
/// All timestamps are Unix time (seconds since Unix epoch, UTC).
/// </summary>
public class RefreshToken : BaseEntity
{
    public string token { get; set; } = string.Empty;

    /// <summary>Unix timestamp (seconds) when this token expires.</summary>
    public long expires_at { get; set; }

    public bool is_revoked { get; set; } = false;
    public string? replaced_by_token { get; set; }
    public string? created_by_ip { get; set; }
    public string? revoked_by_ip { get; set; }

    /// <summary>Unix timestamp (seconds) when this token was revoked. Null if still active.</summary>
    public long? revoked_at { get; set; }

    // FK — int to match User.Id
    public int user_id { get; set; }
    public User user { get; set; } = null!;

    public bool is_active => !is_revoked && DateTimeOffset.UtcNow.ToUnixTimeSeconds() < expires_at;
}
