using Microsoft.EntityFrameworkCore;
using mini_mes_be.Data;
using mini_mes_be.DTOs.Bom;
using mini_mes_be.Models;

namespace mini_mes_be.Services;

public class BomService : IBomService
{
    private readonly AppDbContext _db;

    public BomService(AppDbContext db) => _db = db;

    public async Task<BomResponse> GetByProductIdAsync(int productId)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.id == productId)
            ?? throw new KeyNotFoundException($"Product with ID {productId} not found.");

        var items = await _db.BomItems
            .AsNoTracking()
            .Include(b => b.product)
            .Include(b => b.material)
            .Where(b => b.product_id == productId)
            .OrderBy(b => b.sort_order)
            .ThenBy(b => b.id)
            .Select(b => new BomItemResponse(
                b.id, b.product_id, b.product.name,
                b.material_id, b.material.name, b.material.sku,
                b.quantity, b.unit, b.notes, b.sort_order,
                b.is_active, b.created_at, b.updated_at))
            .ToListAsync();

        return new BomResponse(product.id, product.name, product.sku, items.Count, items);
    }

    public async Task<BomItemResponse> AddItemAsync(int productId, CreateBomItemRequest request)
    {
        var product = await _db.Products.FindAsync(productId)
            ?? throw new KeyNotFoundException($"Product with ID {productId} not found.");

        var material = await _db.Products.FindAsync(request.material_id)
            ?? throw new KeyNotFoundException($"Material with ID {request.material_id} not found.");

        // Prevent self-referencing
        if (productId == request.material_id)
            throw new InvalidOperationException("A product cannot be a material of itself.");

        // Check for circular dependency
        if (await HasCircularDependencyAsync(productId, request.material_id))
            throw new InvalidOperationException(
                $"Adding material '{material.name}' would create a circular BOM dependency.");

        // Check duplicate material in BOM
        if (await _db.BomItems.AnyAsync(b => b.product_id == productId && b.material_id == request.material_id))
            throw new InvalidOperationException(
                $"Material '{material.name}' already exists in this product's BOM.");

        var bomItem = new BomItem
        {
            product_id = productId,
            material_id = request.material_id,
            quantity = request.quantity,
            unit = request.unit,
            notes = request.notes,
            sort_order = request.sort_order
        };

        _db.BomItems.Add(bomItem);
        await _db.SaveChangesAsync();

        return new BomItemResponse(
            bomItem.id, productId, product.name,
            material.id, material.name, material.sku,
            bomItem.quantity, bomItem.unit, bomItem.notes,
            bomItem.sort_order, bomItem.is_active,
            bomItem.created_at, bomItem.updated_at);
    }

    public async Task<BomItemResponse> UpdateItemAsync(int productId, int bomItemId, UpdateBomItemRequest request)
    {
        var bomItem = await _db.BomItems
            .Include(b => b.product)
            .Include(b => b.material)
            .FirstOrDefaultAsync(b => b.id == bomItemId && b.product_id == productId)
            ?? throw new KeyNotFoundException($"BOM item with ID {bomItemId} not found for product {productId}.");

        var material = await _db.Products.FindAsync(request.material_id)
            ?? throw new KeyNotFoundException($"Material with ID {request.material_id} not found.");

        // Prevent self-referencing
        if (productId == request.material_id)
            throw new InvalidOperationException("A product cannot be a material of itself.");

        // If material changed, check circular dependency
        if (bomItem.material_id != request.material_id)
        {
            if (await HasCircularDependencyAsync(productId, request.material_id))
                throw new InvalidOperationException(
                    $"Changing material to '{material.name}' would create a circular BOM dependency.");

            // Check duplicate
            if (await _db.BomItems.AnyAsync(b => b.product_id == productId && b.material_id == request.material_id && b.id != bomItemId))
                throw new InvalidOperationException(
                    $"Material '{material.name}' already exists in this product's BOM.");
        }

        bomItem.material_id = request.material_id;
        bomItem.quantity = request.quantity;
        bomItem.unit = request.unit;
        bomItem.notes = request.notes;
        bomItem.sort_order = request.sort_order;
        bomItem.is_active = request.is_active;
        bomItem.updated_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await _db.SaveChangesAsync();

        return new BomItemResponse(
            bomItem.id, productId, bomItem.product.name,
            material.id, material.name, material.sku,
            bomItem.quantity, bomItem.unit, bomItem.notes,
            bomItem.sort_order, bomItem.is_active,
            bomItem.created_at, bomItem.updated_at);
    }

    public async Task DeleteItemAsync(int productId, int bomItemId)
    {
        var bomItem = await _db.BomItems
            .FirstOrDefaultAsync(b => b.id == bomItemId && b.product_id == productId)
            ?? throw new KeyNotFoundException($"BOM item with ID {bomItemId} not found for product {productId}.");

        _db.BomItems.Remove(bomItem);
        await _db.SaveChangesAsync();
    }

    public async Task<BomResponse> SetBomAsync(int productId, SetBomRequest request)
    {
        var product = await _db.Products.FindAsync(productId)
            ?? throw new KeyNotFoundException($"Product with ID {productId} not found.");

        // Validate all materials exist and no self-reference
        var materialIds = request.items.Select(i => i.material_id).Distinct().ToList();
        if (materialIds.Contains(productId))
            throw new InvalidOperationException("A product cannot be a material of itself.");

        // Check for duplicates in the request
        if (materialIds.Count != request.items.Count)
            throw new InvalidOperationException("Duplicate materials found in the request. Each material should appear only once.");

        var existingMaterials = await _db.Products
            .Where(p => materialIds.Contains(p.id))
            .ToDictionaryAsync(p => p.id);

        foreach (var mid in materialIds)
        {
            if (!existingMaterials.ContainsKey(mid))
                throw new KeyNotFoundException($"Material with ID {mid} not found.");

            if (await HasCircularDependencyAsync(productId, mid))
                throw new InvalidOperationException(
                    $"Adding material '{existingMaterials[mid].name}' would create a circular BOM dependency.");
        }

        // Remove existing BOM items
        var existing = await _db.BomItems.Where(b => b.product_id == productId).ToListAsync();
        _db.BomItems.RemoveRange(existing);

        // Add new items
        var newItems = request.items.Select(item => new BomItem
        {
            product_id = productId,
            material_id = item.material_id,
            quantity = item.quantity,
            unit = item.unit,
            notes = item.notes,
            sort_order = item.sort_order
        }).ToList();

        _db.BomItems.AddRange(newItems);
        await _db.SaveChangesAsync();

        // Re-fetch for complete response
        return await GetByProductIdAsync(productId);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    /// <summary>
    /// Detect circular dependency: checks if 'materialId' (or any of its descendants)
    /// eventually references 'productId' as a material.
    /// </summary>
    private async Task<bool> HasCircularDependencyAsync(int productId, int materialId)
    {
        var visited = new HashSet<int> { productId };
        var queue = new Queue<int>();
        queue.Enqueue(materialId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (visited.Contains(current)) return true;
            visited.Add(current);

            // Find all materials used by 'current' product
            var childMaterials = await _db.BomItems
                .Where(b => b.product_id == current && b.is_active)
                .Select(b => b.material_id)
                .ToListAsync();

            foreach (var child in childMaterials)
            {
                queue.Enqueue(child);
            }
        }

        return false;
    }
}
