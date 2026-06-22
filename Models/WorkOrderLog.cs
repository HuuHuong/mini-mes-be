namespace mini_mes_be.Models;

/// <summary>
/// Audit trail entry for work order lifecycle events.
/// Each status change or production output is recorded here.
/// </summary>
public class WorkOrderLog : BaseEntity
{
    public int work_order_id { get; set; }
    public WorkOrder work_order { get; set; } = null!;

    /// <summary>Event type description (e.g. "StatusChanged", "OutputRecorded").</summary>
    public string event_type { get; set; } = string.Empty;

    /// <summary>Human-readable message describing the event.</summary>
    public string message { get; set; } = string.Empty;

    /// <summary>Previous status value (for status changes).</summary>
    public string? old_value { get; set; }

    /// <summary>New status value (for status changes).</summary>
    public string? new_value { get; set; }

    /// <summary>User who triggered the event.</summary>
    public int? user_id { get; set; }
    public User? user { get; set; }
}
