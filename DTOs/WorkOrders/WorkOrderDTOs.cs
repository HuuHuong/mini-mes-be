using mini_mes_be.Models.Enums;

namespace mini_mes_be.DTOs.WorkOrders;

// ── Requests ──────────────────────────────────────────────────────────────────

public record WorkOrderProductRequest(
    int product_id,
    int target_quantity
);

public record CreateWorkOrderRequest(
    IEnumerable<WorkOrderProductRequest> products,
    int machine_id,
    long? planned_start,
    long? planned_end,
    string? notes
);

public record UpdateWorkOrderRequest(
    IEnumerable<WorkOrderProductRequest> products,
    int machine_id,
    long? planned_start,
    long? planned_end,
    string? notes
);

public record UpdateWorkOrderStatusRequest(
    WorkOrderStatus status
);

public record RecordOutputRequest(
    int product_id,
    int quantity,
    int defect_quantity
);

// ── Responses ─────────────────────────────────────────────────────────────────

public record WorkOrderProductResponse(
    int product_id,
    string product_name,
    string product_sku,
    int target_quantity,
    int produced_quantity,
    int defect_quantity,
    double yield_rate
);

public record WorkOrderResponse(
    int id,
    string order_number,
    IEnumerable<WorkOrderProductResponse> products,
    int machine_id,
    string machine_name,
    string status,
    long? planned_start,
    long? planned_end,
    long? actual_start,
    long? actual_end,
    string? notes,
    string created_by,
    long created_at,
    long? updated_at
);

public record WorkOrderDetailResponse(
    int id,
    string order_number,
    IEnumerable<WorkOrderProductResponse> products,
    int machine_id,
    string machine_name,
    string machine_code,
    string status,
    long? planned_start,
    long? planned_end,
    long? actual_start,
    long? actual_end,
    string? notes,
    string created_by,
    long created_at,
    long? updated_at,
    IEnumerable<WorkOrderLogResponse> logs
);

public record WorkOrderLogResponse(
    int id,
    string event_type,
    string message,
    string? old_value,
    string? new_value,
    string? user_name,
    long created_at
);
