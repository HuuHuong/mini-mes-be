using mini_mes_be.Models.Enums;

namespace mini_mes_be.Models;

/// <summary>
/// Quality inspection record — linked to a work order and a specific product.
/// Records pass/fail results with quantities inspected.
/// </summary>
public class QualityCheck : BaseEntity
{
    public int work_order_id { get; set; }
    public WorkOrder work_order { get; set; } = null!;

    public int product_id { get; set; }
    public Product product { get; set; } = null!;

    /// <summary>Number of items inspected.</summary>
    public int inspected_quantity { get; set; }

    /// <summary>Number of items that passed inspection.</summary>
    public int passed_quantity { get; set; }

    /// <summary>Number of items that failed inspection.</summary>
    public int failed_quantity { get; set; }

    public QualityCheckResult result { get; set; } = QualityCheckResult.Pending;

    public string? notes { get; set; }

    /// <summary>Inspector user.</summary>
    public int inspector_user_id { get; set; }
    public User inspector { get; set; } = null!;
}
