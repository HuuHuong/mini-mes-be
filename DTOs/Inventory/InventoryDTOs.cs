using mini_mes_be.Models.Enums;

namespace mini_mes_be.DTOs.Inventory;

// ── Requests ──────────────────────────────────────────────────────────────────

public record CreateInventoryTransactionRequest(
    int product_id,
    InventoryTransactionType type,
    int quantity,
    int? work_order_id,
    string? reference
);

// ── Responses ─────────────────────────────────────────────────────────────────

public record InventoryStockResponse(
    int product_id,
    string product_name,
    string product_sku,
    string unit,
    int current_stock
);

public record InventoryTransactionResponse(
    int id,
    int product_id,
    string product_name,
    string type,
    int quantity,
    int? work_order_id,
    string? order_number,
    string? reference,
    string user_name,
    long created_at
);
