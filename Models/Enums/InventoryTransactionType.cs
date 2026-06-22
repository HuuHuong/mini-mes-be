namespace mini_mes_be.Models.Enums;

/// <summary>
/// Categorizes stock movement transactions.
/// </summary>
public enum InventoryTransactionType
{
    /// <summary>Stock produced from a work order.</summary>
    Production = 0,
    /// <summary>Manual stock adjustment (+ or -).</summary>
    Adjustment = 1,
    /// <summary>Outbound shipment to customer.</summary>
    Shipment = 2,
    /// <summary>Returned goods.</summary>
    Return = 3
}
