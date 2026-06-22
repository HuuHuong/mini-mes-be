using mini_mes_be.Models.Enums;

namespace mini_mes_be.Models;

/// <summary>
/// Records a stock movement (in or out) for a product.
/// Every production output, adjustment, or shipment creates a transaction.
/// </summary>
public class InventoryTransaction : BaseEntity
{
    public int product_id { get; set; }
    public Product product { get; set; } = null!;

    public InventoryTransactionType type { get; set; }

    /// <summary>Positive = stock in, Negative = stock out.</summary>
    public int quantity { get; set; }

    /// <summary>Optional reference to the source work order.</summary>
    public int? work_order_id { get; set; }
    public WorkOrder? work_order { get; set; }

    /// <summary>Reference note (e.g. "WO-001 output", "Manual adjustment").</summary>
    public string? reference { get; set; }

    /// <summary>User who performed the transaction.</summary>
    public int user_id { get; set; }
    public User user { get; set; } = null!;
}
