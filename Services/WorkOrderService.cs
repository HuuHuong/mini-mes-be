using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using mini_mes_be.Data;
using mini_mes_be.DTOs.Pagination;
using mini_mes_be.DTOs.WorkOrders;
using mini_mes_be.Hubs;
using mini_mes_be.Models;
using mini_mes_be.Models.Enums;

namespace mini_mes_be.Services;

public class WorkOrderService : IWorkOrderService
{
    private readonly AppDbContext _db;
    private readonly IHubContext<MesHub> _hub;

    public WorkOrderService(AppDbContext db, IHubContext<MesHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    public async Task<PaginatedResponse<WorkOrderResponse>> GetAllAsync(PaginatedRequest request, WorkOrderStatus? statusFilter)
    {
        var query = _db.WorkOrders.AsNoTracking()
            .Include(wo => wo.products).ThenInclude(wop => wop.product)
            .Include(wo => wo.machine)
            .Include(wo => wo.created_by_user)
            .AsQueryable();

        if (statusFilter.HasValue)
            query = query.Where(wo => wo.status == statusFilter.Value);

        if (!string.IsNullOrWhiteSpace(request.search))
        {
            var search = request.search.Trim().ToLower();
            query = query.Where(wo =>
                wo.order_number.ToLower().Contains(search) ||
                wo.products.Any(wop => wop.product.name.ToLower().Contains(search)));
        }

        query = request.sort_by?.ToLower() switch
        {
            "order_number" => request.sort_direction == "desc" ? query.OrderByDescending(wo => wo.order_number) : query.OrderBy(wo => wo.order_number),
            "status" => request.sort_direction == "desc" ? query.OrderByDescending(wo => wo.status) : query.OrderBy(wo => wo.status),
            _ => query.OrderByDescending(wo => wo.created_at)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((request.page - 1) * request.page_size)
            .Take(request.page_size)
            .Select(wo => MapToResponse(wo))
            .ToListAsync();

        return PaginatedResponse<WorkOrderResponse>.Create(items, totalCount, request.page, request.page_size);
    }

    public async Task<WorkOrderDetailResponse> GetByIdAsync(int id)
    {
        var wo = await _db.WorkOrders.AsNoTracking()
            .Include(wo => wo.products).ThenInclude(wop => wop.product)
            .Include(w => w.machine)
            .Include(w => w.created_by_user)
            .Include(w => w.logs).ThenInclude(l => l.user)
            .FirstOrDefaultAsync(w => w.id == id)
            ?? throw new KeyNotFoundException($"Work order with ID {id} not found.");

        return MapToDetailResponse(wo);
    }

    public async Task<WorkOrderResponse> CreateAsync(CreateWorkOrderRequest request, int userId)
    {
        if (request.products == null || !request.products.Any())
            throw new ArgumentException("Work order must have at least one product.");

        if (!await _db.Machines.AnyAsync(m => m.id == request.machine_id && m.is_active))
            throw new KeyNotFoundException($"Machine with ID {request.machine_id} not found or inactive.");

        foreach (var reqProd in request.products)
        {
            if (!await _db.Products.AnyAsync(p => p.id == reqProd.product_id && p.is_active))
                throw new KeyNotFoundException($"Product with ID {reqProd.product_id} not found or inactive.");
        }

        // Generate order number
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var count = await _db.WorkOrders.CountAsync(wo => wo.order_number.StartsWith($"WO-{today}"));
        var orderNumber = $"WO-{today}-{(count + 1):D3}";

        var wo = new WorkOrder
        {
            order_number = orderNumber,
            machine_id = request.machine_id,
            planned_start = request.planned_start,
            planned_end = request.planned_end,
            notes = request.notes,
            created_by_user_id = userId,
            status = WorkOrderStatus.Pending
        };

        _db.WorkOrders.Add(wo);

        foreach (var reqProd in request.products)
        {
            _db.WorkOrderProducts.Add(new WorkOrderProduct
            {
                work_order = wo,
                product_id = reqProd.product_id,
                target_quantity = reqProd.target_quantity,
                produced_quantity = 0,
                defect_quantity = 0
            });
        }

        // Add creation log
        _db.WorkOrderLogs.Add(new WorkOrderLog
        {
            work_order = wo,
            event_type = "Created",
            message = $"Work order {orderNumber} created with {request.products.Count()} products",
            new_value = WorkOrderStatus.Pending.ToString(),
            user_id = userId
        });

        await _db.SaveChangesAsync();

        // Reload with navigation properties
        var reloadedWo = await _db.WorkOrders
            .Include(w => w.products).ThenInclude(wop => wop.product)
            .Include(w => w.machine)
            .Include(w => w.created_by_user)
            .FirstOrDefaultAsync(w => w.id == wo.id);

        return MapToResponse(reloadedWo!);
    }

    public async Task<WorkOrderResponse> UpdateAsync(int id, UpdateWorkOrderRequest request)
    {
        if (request.products == null || !request.products.Any())
            throw new ArgumentException("Work order must have at least one product.");

        var wo = await _db.WorkOrders
            .Include(w => w.products).ThenInclude(wop => wop.product)
            .Include(w => w.machine).Include(w => w.created_by_user)
            .FirstOrDefaultAsync(w => w.id == id)
            ?? throw new KeyNotFoundException($"Work order with ID {id} not found.");

        if (wo.status == WorkOrderStatus.Completed || wo.status == WorkOrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot update a completed or cancelled work order.");

        if (!await _db.Machines.AnyAsync(m => m.id == request.machine_id && m.is_active))
            throw new KeyNotFoundException($"Machine with ID {request.machine_id} not found or inactive.");

        foreach (var reqProd in request.products)
        {
            if (!await _db.Products.AnyAsync(p => p.id == reqProd.product_id && p.is_active))
                throw new KeyNotFoundException($"Product with ID {reqProd.product_id} not found or inactive.");
        }

        wo.machine_id = request.machine_id;
        wo.planned_start = request.planned_start;
        wo.planned_end = request.planned_end;
        wo.notes = request.notes;
        wo.updated_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Remove old products and add new ones
        _db.WorkOrderProducts.RemoveRange(wo.products);

        foreach (var reqProd in request.products)
        {
            _db.WorkOrderProducts.Add(new WorkOrderProduct
            {
                work_order_id = wo.id,
                product_id = reqProd.product_id,
                target_quantity = reqProd.target_quantity,
                produced_quantity = 0,
                defect_quantity = 0
            });
        }

        await _db.SaveChangesAsync();

        var reloadedWo = await _db.WorkOrders
            .Include(w => w.products).ThenInclude(wop => wop.product)
            .Include(w => w.machine)
            .Include(w => w.created_by_user)
            .FirstOrDefaultAsync(w => w.id == wo.id);

        return MapToResponse(reloadedWo!);
    }

    public async Task<WorkOrderResponse> UpdateStatusAsync(int id, UpdateWorkOrderStatusRequest request, int userId)
    {
        var wo = await _db.WorkOrders
            .Include(w => w.products).ThenInclude(wop => wop.product)
            .Include(w => w.machine).Include(w => w.created_by_user)
            .FirstOrDefaultAsync(w => w.id == id)
            ?? throw new KeyNotFoundException($"Work order with ID {id} not found.");

        var oldStatus = wo.status;
        wo.status = request.status;
        wo.updated_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Set actual timestamps based on status
        if (request.status == WorkOrderStatus.InProgress && wo.actual_start == null)
            wo.actual_start = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (request.status == WorkOrderStatus.Completed || request.status == WorkOrderStatus.Cancelled)
            wo.actual_end = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Add status change log
        _db.WorkOrderLogs.Add(new WorkOrderLog
        {
            work_order_id = wo.id,
            event_type = "StatusChanged",
            message = $"Status changed from {oldStatus} to {request.status}",
            old_value = oldStatus.ToString(),
            new_value = request.status.ToString(),
            user_id = userId
        });

        await _db.SaveChangesAsync();

        // ── SignalR broadcast ──
        await _hub.Clients.All.SendAsync("WorkOrderStatusChanged", new
        {
            work_order_id = wo.id,
            order_number = wo.order_number,
            old_status = oldStatus.ToString(),
            new_status = request.status.ToString(),
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        });

        return MapToResponse(wo);
    }

    public async Task<WorkOrderResponse> RecordOutputAsync(int id, RecordOutputRequest request, int userId)
    {
        var wo = await _db.WorkOrders
            .Include(w => w.products).ThenInclude(wop => wop.product)
            .Include(w => w.machine).Include(w => w.created_by_user)
            .FirstOrDefaultAsync(w => w.id == id)
            ?? throw new KeyNotFoundException($"Work order with ID {id} not found.");

        if (wo.status != WorkOrderStatus.InProgress)
            throw new InvalidOperationException("Can only record output for work orders that are In Progress.");

        var woProduct = wo.products.FirstOrDefault(p => p.product_id == request.product_id)
            ?? throw new KeyNotFoundException($"Product with ID {request.product_id} is not associated with this work order.");

        woProduct.produced_quantity += request.quantity;
        woProduct.defect_quantity += request.defect_quantity;
        wo.updated_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Auto-complete if ALL products reached their targets
        if (wo.products.All(p => p.produced_quantity >= p.target_quantity))
        {
            wo.status = WorkOrderStatus.Completed;
            wo.actual_end = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        // Add log
        _db.WorkOrderLogs.Add(new WorkOrderLog
        {
            work_order_id = wo.id,
            event_type = "OutputRecorded",
            message = $"Recorded output for {woProduct.product.name}: Produced {request.quantity} (defect: {request.defect_quantity}). Total: {woProduct.produced_quantity}/{woProduct.target_quantity}",
            new_value = $"{woProduct.produced_quantity}/{woProduct.target_quantity}",
            user_id = userId
        });

        // Create inventory transaction for good output
        var goodQuantity = request.quantity - request.defect_quantity;
        if (goodQuantity > 0)
        {
            _db.InventoryTransactions.Add(new InventoryTransaction
            {
                product_id = request.product_id,
                type = InventoryTransactionType.Production,
                quantity = goodQuantity,
                work_order_id = wo.id,
                reference = $"{wo.order_number} production output ({woProduct.product.name})",
                user_id = userId
            });
        }

        await _db.SaveChangesAsync();

        // ── SignalR broadcast ──
        await _hub.Clients.All.SendAsync("ProductionOutputUpdated", new
        {
            work_order_id = wo.id,
            order_number = wo.order_number,
            product_id = request.product_id,
            product_name = woProduct.product.name,
            produced_quantity = woProduct.produced_quantity,
            target_quantity = woProduct.target_quantity,
            defect_quantity = woProduct.defect_quantity,
            status = wo.status.ToString(),
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        });

        return MapToResponse(wo);
    }

    private static WorkOrderResponse MapToResponse(WorkOrder wo) => new(
        wo.id, wo.order_number,
        wo.products.Select(p => new WorkOrderProductResponse(
            p.product_id,
            p.product.name,
            p.product.sku,
            p.target_quantity,
            p.produced_quantity,
            p.defect_quantity,
            p.produced_quantity > 0 ? Math.Round((double)(p.produced_quantity - p.defect_quantity) / p.produced_quantity * 100, 2) : 0
        )),
        wo.machine_id, wo.machine.name,
        wo.status.ToString(),
        wo.planned_start, wo.planned_end,
        wo.actual_start, wo.actual_end,
        wo.notes, wo.created_by_user.username,
        wo.created_at, wo.updated_at);

    private static WorkOrderDetailResponse MapToDetailResponse(WorkOrder wo) => new(
        wo.id, wo.order_number,
        wo.products.Select(p => new WorkOrderProductResponse(
            p.product_id,
            p.product.name,
            p.product.sku,
            p.target_quantity,
            p.produced_quantity,
            p.defect_quantity,
            p.produced_quantity > 0 ? Math.Round((double)(p.produced_quantity - p.defect_quantity) / p.produced_quantity * 100, 2) : 0
        )),
        wo.machine_id, wo.machine.name, wo.machine.code,
        wo.status.ToString(),
        wo.planned_start, wo.planned_end,
        wo.actual_start, wo.actual_end,
        wo.notes, wo.created_by_user.username,
        wo.created_at, wo.updated_at,
        wo.logs.OrderByDescending(l => l.created_at).Select(l => new WorkOrderLogResponse(
            l.id, l.event_type, l.message,
            l.old_value, l.new_value,
            l.user?.username, l.created_at)));
}
