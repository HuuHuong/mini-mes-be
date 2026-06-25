using mini_mes_be.DTOs.Bom;

namespace mini_mes_be.Services;

public interface IBomService
{
    /// <summary>Get the full BOM for a product.</summary>
    Task<BomResponse> GetByProductIdAsync(int productId);

    /// <summary>Add a single item to a product's BOM.</summary>
    Task<BomItemResponse> AddItemAsync(int productId, CreateBomItemRequest request);

    /// <summary>Update a BOM item.</summary>
    Task<BomItemResponse> UpdateItemAsync(int productId, int bomItemId, UpdateBomItemRequest request);

    /// <summary>Remove a BOM item.</summary>
    Task DeleteItemAsync(int productId, int bomItemId);

    /// <summary>Replace the entire BOM for a product (batch set).</summary>
    Task<BomResponse> SetBomAsync(int productId, SetBomRequest request);
}
