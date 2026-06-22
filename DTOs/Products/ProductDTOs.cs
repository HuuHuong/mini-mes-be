namespace mini_mes_be.DTOs.Products;

// ── Requests ──────────────────────────────────────────────────────────────────

public record CreateProductRequest(
    string name,
    string sku,
    string unit,
    string? description
);

public record UpdateProductRequest(
    string name,
    string sku,
    string unit,
    string? description,
    bool is_active
);

// ── Responses ─────────────────────────────────────────────────────────────────

public record ProductResponse(
    int id,
    string name,
    string sku,
    string unit,
    string? description,
    bool is_active,
    long created_at,
    long? updated_at
);

public record ProductDetailResponse(
    int id,
    string name,
    string sku,
    string unit,
    string? description,
    bool is_active,
    int current_stock,
    int active_work_orders,
    long created_at,
    long? updated_at
);
