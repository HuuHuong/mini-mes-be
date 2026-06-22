namespace mini_mes_be.Models.Enums;

/// <summary>
/// Represents the operational status of a production machine.
/// </summary>
public enum MachineStatus
{
    Idle = 0,
    Running = 1,
    Maintenance = 2,
    Error = 3
}
