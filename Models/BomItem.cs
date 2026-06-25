namespace mini_mes_be.Models;

/// <summary>
/// Bill of Materials (BOM) item — defines a material/component required to produce a parent product.
/// Each BOM item links a parent product to a child material product with a required quantity.
/// </summary>
public class BomItem : BaseEntity
{
    /// <summary>The finished/parent product being manufactured.</summary>
    public int product_id { get; set; }
    public Product product { get; set; } = null!;

    /// <summary>The raw material or component required.</summary>
    public int material_id { get; set; }
    public Product material { get; set; } = null!;

    /// <summary>Quantity of material required to produce one unit of the parent product.</summary>
    public decimal quantity { get; set; }

    /// <summary>Unit of measure for this BOM line (e.g. "pcs", "kg", "m").</summary>
    public string unit { get; set; } = "pcs";

    /// <summary>Optional notes for this BOM line.</summary>
    public string? notes { get; set; }

    /// <summary>Position/order of this item in the BOM list.</summary>
    public int sort_order { get; set; } = 0;

    /// <summary>Whether this BOM item is active.</summary>
    public bool is_active { get; set; } = true;
}
