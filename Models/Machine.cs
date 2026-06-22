using mini_mes_be.Models.Enums;

namespace mini_mes_be.Models;

/// <summary>
/// Represents a physical production machine on the shop floor.
/// </summary>
public class Machine : BaseEntity
{
    public string name { get; set; } = string.Empty;

    /// <summary>Unique machine code (e.g. "CNC-001").</summary>
    public string code { get; set; } = string.Empty;

    /// <summary>Physical location description (e.g. "Hall A, Line 2").</summary>
    public string? location { get; set; }

    public MachineStatus status { get; set; } = MachineStatus.Idle;

    public bool is_active { get; set; } = true;

    // Navigation
    public ICollection<WorkOrder> work_orders { get; set; } = [];
}
