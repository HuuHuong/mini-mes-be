using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using mini_mes_be.Data;
using mini_mes_be.DTOs.Inventory;
using mini_mes_be.DTOs.Pagination;
using mini_mes_be.Hubs;
using mini_mes_be.Models;

namespace mini_mes_be.Services;

public class InventoryService : IInventoryService
{
    private readonly AppDbContext _db;
    private readonly IHubContext<MesHub> _hub;

    public InventoryService(AppDbContext db, IHubContext<MesHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    public async Task<PaginatedResponse<InventoryStockResponse>> GetStockSummaryAsync(PaginatedRequest request)
    {
        var query = _db.Products.AsNoTracking().Where(p => p.is_active);

        if (!string.IsNullOrWhiteSpace(request.search))
        {
            var search = request.search.Trim().ToLower();
            query = query.Where(p => p.name.ToLower().Contains(search) || p.sku.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.name)
            .Skip((request.page - 1) * request.page_size)
            .Take(request.page_size)
            .Select(p => new InventoryStockResponse(
                p.id, p.name, p.sku, p.unit,
                p.inventory_transactions.Sum(it => it.quantity)))
            .ToListAsync();

        return PaginatedResponse<InventoryStockResponse>.Create(items, totalCount, request.page, request.page_size);
    }

    public async Task<PaginatedResponse<InventoryTransactionResponse>> GetTransactionsAsync(PaginatedRequest request, int? productId)
    {
        var query = _db.InventoryTransactions.AsNoTracking()
            .Include(it => it.product)
            .Include(it => it.work_order)
            .Include(it => it.user)
            .AsQueryable();

        if (productId.HasValue)
            query = query.Where(it => it.product_id == productId.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(it => it.created_at)
            .Skip((request.page - 1) * request.page_size)
            .Take(request.page_size)
            .Select(it => new InventoryTransactionResponse(
                it.id, it.product_id, it.product.name,
                it.type.ToString(), it.quantity,
                it.work_order_id, it.work_order != null ? it.work_order.order_number : null,
                it.reference, it.user.username, it.created_at))
            .ToListAsync();

        return PaginatedResponse<InventoryTransactionResponse>.Create(items, totalCount, request.page, request.page_size);
    }

    public async Task<InventoryTransactionResponse> CreateTransactionAsync(CreateInventoryTransactionRequest request, int userId)
    {
        var product = await _db.Products.FindAsync(request.product_id)
            ?? throw new KeyNotFoundException($"Product with ID {request.product_id} not found.");

        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var transaction = new InventoryTransaction
        {
            product_id = request.product_id,
            type = request.type,
            quantity = request.quantity,
            work_order_id = request.work_order_id,
            reference = request.reference,
            user_id = userId
        };

        _db.InventoryTransactions.Add(transaction);
        await _db.SaveChangesAsync();

        // ── SignalR broadcast ──
        var newStock = await _db.InventoryTransactions
            .Where(it => it.product_id == request.product_id)
            .SumAsync(it => it.quantity);

        await _hub.Clients.All.SendAsync("InventoryChanged", new
        {
            product_id = request.product_id,
            product_name = product.name,
            transaction_type = request.type.ToString(),
            quantity = request.quantity,
            new_stock = newStock,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        });

        WorkOrder? wo = null;
        if (request.work_order_id.HasValue)
            wo = await _db.WorkOrders.FindAsync(request.work_order_id.Value);

        return new InventoryTransactionResponse(
            transaction.id, transaction.product_id, product.name,
            transaction.type.ToString(), transaction.quantity,
            transaction.work_order_id, wo?.order_number,
            transaction.reference, user.username, transaction.created_at);
    }
}
