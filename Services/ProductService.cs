using Microsoft.EntityFrameworkCore;
using mini_mes_be.Data;
using mini_mes_be.DTOs.Pagination;
using mini_mes_be.DTOs.Products;
using mini_mes_be.Middlewares;
using mini_mes_be.Models;
using mini_mes_be.Models.Enums;

namespace mini_mes_be.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db) => _db = db;

    public async Task<PaginatedResponse<ProductResponse>> GetAllAsync(PaginatedRequest request)
    {
        var query = _db.Products.AsNoTracking().AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(request.search))
        {
            var search = request.search.Trim().ToLower();
            query = query.Where(p =>
                p.name.ToLower().Contains(search) ||
                p.sku.ToLower().Contains(search));
        }

        // Sort
        query = request.sort_by?.ToLower() switch
        {
            "name" => request.sort_direction == "desc" ? query.OrderByDescending(p => p.name) : query.OrderBy(p => p.name),
            "sku" => request.sort_direction == "desc" ? query.OrderByDescending(p => p.sku) : query.OrderBy(p => p.sku),
            "created_at" => request.sort_direction == "desc" ? query.OrderByDescending(p => p.created_at) : query.OrderBy(p => p.created_at),
            _ => query.OrderByDescending(p => p.created_at)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((request.page - 1) * request.page_size)
            .Take(request.page_size)
            .Select(p => new ProductResponse(
                p.id, p.name, p.sku, p.unit, p.description,
                p.is_active, p.created_at, p.updated_at))
            .ToListAsync();

        return PaginatedResponse<ProductResponse>.Create(items, totalCount, request.page, request.page_size);
    }

    public async Task<ProductDetailResponse> GetByIdAsync(int id)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.id == id)
            ?? throw new KeyNotFoundException($"Product with ID {id} not found.");

        var currentStock = await _db.InventoryTransactions
            .Where(it => it.product_id == id)
            .SumAsync(it => it.quantity);

        var activeWoCount = await _db.WorkOrders
            .CountAsync(wo => wo.products.Any(p => p.product_id == id) &&
                (wo.status == WorkOrderStatus.Pending || wo.status == WorkOrderStatus.InProgress));

        return new ProductDetailResponse(
            product.id, product.name, product.sku, product.unit,
            product.description, product.is_active,
            currentStock, activeWoCount,
            product.created_at, product.updated_at);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        if (await _db.Products.AnyAsync(p => p.sku == request.sku))
            throw new InvalidOperationException($"A product with SKU '{request.sku}' already exists.");

        var product = new Product
        {
            name = request.name,
            sku = request.sku,
            unit = request.unit,
            description = request.description
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return new ProductResponse(
            product.id, product.name, product.sku, product.unit,
            product.description, product.is_active,
            product.created_at, product.updated_at);
    }

    public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await _db.Products.FindAsync(id)
            ?? throw new KeyNotFoundException($"Product with ID {id} not found.");

        // Check SKU uniqueness (exclude self)
        if (await _db.Products.AnyAsync(p => p.sku == request.sku && p.id != id))
            throw new InvalidOperationException($"A product with SKU '{request.sku}' already exists.");

        product.name = request.name;
        product.sku = request.sku;
        product.unit = request.unit;
        product.description = request.description;
        product.is_active = request.is_active;
        product.updated_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await _db.SaveChangesAsync();

        return new ProductResponse(
            product.id, product.name, product.sku, product.unit,
            product.description, product.is_active,
            product.created_at, product.updated_at);
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _db.Products.FindAsync(id)
            ?? throw new KeyNotFoundException($"Product with ID {id} not found.");

        // Soft delete
        product.is_active = false;
        product.updated_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await _db.SaveChangesAsync();
    }
}
