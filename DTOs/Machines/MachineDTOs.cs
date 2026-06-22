using mini_mes_be.Models.Enums;

namespace mini_mes_be.DTOs.Machines;

// ── Requests ──────────────────────────────────────────────────────────────────

public record CreateMachineRequest(
    string name,
    string code,
    string? location
);

public record UpdateMachineRequest(
    string name,
    string code,
    string? location,
    bool is_active
);

public record UpdateMachineStatusRequest(
    MachineStatus status
);

// ── Responses ─────────────────────────────────────────────────────────────────

public record MachineResponse(
    int id,
    string name,
    string code,
    string? location,
    string status,
    bool is_active,
    long created_at,
    long? updated_at
);

public record MachineDetailResponse(
    int id,
    string name,
    string code,
    string? location,
    string status,
    bool is_active,
    int active_work_orders,
    long created_at,
    long? updated_at
);
