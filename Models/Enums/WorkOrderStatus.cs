namespace mini_mes_be.Models.Enums;

/// <summary>
/// Represents the lifecycle status of a work order.
/// </summary>
public enum WorkOrderStatus
{
    Pending = 0,
    InProgress = 1,
    Paused = 2,
    Completed = 3,
    Cancelled = 4
}
