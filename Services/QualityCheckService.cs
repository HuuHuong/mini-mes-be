using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using mini_mes_be.Data;
using mini_mes_be.DTOs.Pagination;
using mini_mes_be.DTOs.QualityChecks;
using mini_mes_be.Hubs;
using mini_mes_be.Models;
using mini_mes_be.Models.Enums;

namespace mini_mes_be.Services;

public class QualityCheckService : IQualityCheckService
{
    private readonly AppDbContext _db;
    private readonly IHubContext<MesHub> _hub;

    public QualityCheckService(AppDbContext db, IHubContext<MesHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    public async Task<PaginatedResponse<QualityCheckResponse>> GetAllAsync(
        PaginatedRequest request, int? workOrderId, QualityCheckResult? resultFilter)
    {
        var query = _db.QualityChecks.AsNoTracking()
            .Include(qc => qc.work_order)
            .Include(qc => qc.product)
            .Include(qc => qc.inspector)
            .AsQueryable();

        if (workOrderId.HasValue)
            query = query.Where(qc => qc.work_order_id == workOrderId.Value);

        if (resultFilter.HasValue)
            query = query.Where(qc => qc.result == resultFilter.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(qc => qc.created_at)
            .Skip((request.page - 1) * request.page_size)
            .Take(request.page_size)
            .Select(qc => new QualityCheckResponse(
                qc.id, qc.work_order_id, qc.work_order.order_number,
                qc.product_id, qc.product.name,
                qc.inspected_quantity, qc.passed_quantity, qc.failed_quantity,
                qc.result.ToString(), qc.notes,
                qc.inspector.username, qc.created_at))
            .ToListAsync();

        return PaginatedResponse<QualityCheckResponse>.Create(items, totalCount, request.page, request.page_size);
    }

    public async Task<QualityCheckResponse> CreateAsync(CreateQualityCheckRequest request, int userId)
    {
        var wo = await _db.WorkOrders
            .Include(w => w.products)
            .FirstOrDefaultAsync(w => w.id == request.work_order_id)
            ?? throw new KeyNotFoundException($"Work order with ID {request.work_order_id} not found.");

        var product = await _db.Products.FindAsync(request.product_id)
            ?? throw new KeyNotFoundException($"Product with ID {request.product_id} not found.");

        var wop = wo.products.FirstOrDefault(p => p.product_id == request.product_id)
            ?? throw new KeyNotFoundException($"Product with ID {request.product_id} is not associated with this work order.");

        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var qc = new QualityCheck
        {
            work_order_id = request.work_order_id,
            product_id = request.product_id,
            inspected_quantity = request.inspected_quantity,
            passed_quantity = request.passed_quantity,
            failed_quantity = request.failed_quantity,
            result = request.result,
            notes = request.notes,
            inspector_user_id = userId
        };

        _db.QualityChecks.Add(qc);

        // Update work order product defect count
        wop.defect_quantity += request.failed_quantity;
        wo.updated_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Add log to work order
        _db.WorkOrderLogs.Add(new WorkOrderLog
        {
            work_order_id = wo.id,
            event_type = "QualityCheck",
            message = $"QC for {product.name}: {request.passed_quantity} passed, {request.failed_quantity} failed ({request.result})",
            new_value = request.result.ToString(),
            user_id = userId
        });

        await _db.SaveChangesAsync();

        // ── SignalR broadcast ──
        await _hub.Clients.All.SendAsync("QualityCheckRecorded", new
        {
            quality_check_id = qc.id,
            work_order_id = wo.id,
            order_number = wo.order_number,
            product_id = request.product_id,
            product_name = product.name,
            result = request.result.ToString(),
            passed = request.passed_quantity,
            failed = request.failed_quantity,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        });

        return new QualityCheckResponse(
            qc.id, qc.work_order_id, wo.order_number,
            qc.product_id, product.name,
            qc.inspected_quantity, qc.passed_quantity, qc.failed_quantity,
            qc.result.ToString(), qc.notes,
            user.username, qc.created_at);
    }
}
