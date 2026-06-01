namespace mini_mes_be.Models;

/// <summary>
/// Base entity with common audit fields.
/// All domain models should inherit from this.
/// - Id: auto-increment integer primary key (IDENTITY in SQL Server).
/// - Timestamps are stored as Unix time (seconds since Unix epoch, UTC).
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Auto-increment integer primary key.</summary>
    public int Id { get; set; }

    /// <summary>Unix timestamp (seconds) when the record was created.</summary>
    public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    /// <summary>Unix timestamp (seconds) when the record was last updated. Null if never updated.</summary>
    public long? UpdatedAt { get; set; }
}
