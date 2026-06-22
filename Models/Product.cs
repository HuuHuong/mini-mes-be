namespace mini_mes_be.Models;

/// <summary>
/// Product catalog entry — represents a type of product that can be manufactured.
/// </summary>
public class Product : BaseEntity
{
    public string name { get; set; } = string.Empty;

    /// <summary>Stock Keeping Unit — unique product identifier code.</summary>
    public string sku { get; set; } = string.Empty;

    /// <summary>Unit of measure (e.g. "pcs", "kg", "m").</summary>
    public string unit { get; set; } = "pcs";

    public string? description { get; set; }

    public bool is_active { get; set; } = true;

    // Navigation
    public ICollection<WorkOrderProduct> work_order_products { get; set; } = [];
    public ICollection<InventoryTransaction> inventory_transactions { get; set; } = [];
}
