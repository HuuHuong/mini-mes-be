namespace mini_mes_be.DTOs.Bom;

// ── Requests ──────────────────────────────────────────────────────────────────

public record CreateBomItemRequest(
    int material_id,
    decimal quantity,
    string unit,
    string? notes,
    int sort_order
);

public record UpdateBomItemRequest(
    int material_id,
    decimal quantity,
    string unit,
    string? notes,
    int sort_order,
    bool is_active
);

/// <summary>Batch request to set the entire BOM for a product at once.</summary>
public record SetBomRequest(
    List<CreateBomItemRequest> items
);

// ── Responses ─────────────────────────────────────────────────────────────────

public record BomItemResponse(
    int id,
    int product_id,
    string product_name,
    int material_id,
    string material_name,
    string material_sku,
    decimal quantity,
    string unit,
    string? notes,
    int sort_order,
    bool is_active,
    long created_at,
    long? updated_at
);

/// <summary>Full BOM for a product, including all items.</summary>
public record BomResponse(
    int product_id,
    string product_name,
    string product_sku,
    int total_items,
    IEnumerable<BomItemResponse> items
);
