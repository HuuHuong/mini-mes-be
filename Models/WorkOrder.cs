using mini_mes_be.Models.Enums;

namespace mini_mes_be.Models;

/// <summary>
/// A production work order — the core scheduling unit in MES.
/// Can run multiple Products on a Machine (line) simultaneously or sequentially.
/// </summary>
public class WorkOrder : BaseEntity
{
    /// <summary>Human-readable order number (e.g. "WO-20260620-001").</summary>
    public string order_number { get; set; } = string.Empty;

    public int machine_id { get; set; }
    public Machine machine { get; set; } = null!;

    public WorkOrderStatus status { get; set; } = WorkOrderStatus.Pending;

    /// <summary>Planned start date (Unix timestamp seconds).</summary>
    public long? planned_start { get; set; }

    /// <summary>Planned end date (Unix timestamp seconds).</summary>
    public long? planned_end { get; set; }

    /// <summary>Actual start date (Unix timestamp seconds).</summary>
    public long? actual_start { get; set; }

    /// <summary>Actual end date (Unix timestamp seconds).</summary>
    public long? actual_end { get; set; }

    public string? notes { get; set; }

    /// <summary>User who created this work order.</summary>
    public int created_by_user_id { get; set; }
    public User created_by_user { get; set; } = null!;

    // Navigation
    public ICollection<WorkOrderProduct> products { get; set; } = [];
    public ICollection<WorkOrderLog> logs { get; set; } = [];
    public ICollection<QualityCheck> quality_checks { get; set; } = [];
}
