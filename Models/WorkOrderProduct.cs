namespace mini_mes_be.Models;

/// <summary>
/// Represents a product running on a work order with its own target and produced quantities.
/// </summary>
public class WorkOrderProduct : BaseEntity
{
    public int work_order_id { get; set; }
    public WorkOrder work_order { get; set; } = null!;

    public int product_id { get; set; }
    public Product product { get; set; } = null!;

    public int target_quantity { get; set; }
    public int produced_quantity { get; set; } = 0;
    public int defect_quantity { get; set; } = 0;
}
