using mini_mes_be.Models.Enums;

namespace mini_mes_be.DTOs.QualityChecks;

// ── Requests ──────────────────────────────────────────────────────────────────

public record CreateQualityCheckRequest(
    int work_order_id,
    int product_id,
    int inspected_quantity,
    int passed_quantity,
    int failed_quantity,
    QualityCheckResult result,
    string? notes
);

// ── Responses ─────────────────────────────────────────────────────────────────

public record QualityCheckResponse(
    int id,
    int work_order_id,
    string order_number,
    int product_id,
    string product_name,
    int inspected_quantity,
    int passed_quantity,
    int failed_quantity,
    string result,
    string? notes,
    string inspector_name,
    long created_at
);
