using Microsoft.EntityFrameworkCore;
using mini_mes_be.Data;
using mini_mes_be.DTOs.Dashboard;
using mini_mes_be.Models.Enums;

namespace mini_mes_be.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db) => _db = db;

    public async Task<DashboardSummaryResponse> GetSummaryAsync()
    {
        var todayStart = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero).ToUnixTimeSeconds();

        // Product & machine counts
        var totalProducts = await _db.Products.CountAsync(p => p.is_active);
        var machines = await _db.Machines.Where(m => m.is_active).ToListAsync();
        var totalMachines = machines.Count;
        var machinesRunning = machines.Count(m => m.status == MachineStatus.Running);
        var machinesIdle = machines.Count(m => m.status == MachineStatus.Idle);
        var machinesMaint = machines.Count(m => m.status == MachineStatus.Maintenance);
        var machinesError = machines.Count(m => m.status == MachineStatus.Error);

        // Work order stats
        var totalWo = await _db.WorkOrders.CountAsync();
        var woPending = await _db.WorkOrders.CountAsync(wo => wo.status == WorkOrderStatus.Pending);
        var woInProgress = await _db.WorkOrders.CountAsync(wo => wo.status == WorkOrderStatus.InProgress);
        var woCompletedToday = await _db.WorkOrders.CountAsync(wo =>
            wo.status == WorkOrderStatus.Completed && wo.actual_end != null && wo.actual_end >= todayStart);

        // Today's output
        var todayOutput = await _db.WorkOrderLogs
            .Where(l => l.event_type == "OutputRecorded" && l.created_at >= todayStart)
            .CountAsync();

        // Today's production stats from inventory
        var todayProduced = await _db.InventoryTransactions
            .Where(it => it.type == InventoryTransactionType.Production && it.created_at >= todayStart)
            .SumAsync(it => it.quantity);

        var todayDefects = await _db.WorkOrderProducts
            .Where(wop => wop.work_order.updated_at != null && wop.work_order.updated_at >= todayStart)
            .SumAsync(wop => wop.defect_quantity);

        var todayYieldRate = todayProduced + todayDefects > 0
            ? Math.Round((double)todayProduced / (todayProduced + todayDefects) * 100, 2)
            : 100.0;

        // Recent work orders (last 10)
        var recentWoDb = await _db.WorkOrders.AsNoTracking()
            .Include(wo => wo.products).ThenInclude(wop => wop.product)
            .Include(wo => wo.machine)
            .OrderByDescending(wo => wo.created_at)
            .Take(10)
            .ToListAsync();

        var recentWo = recentWoDb.Select(wo => new RecentWorkOrderResponse(
            wo.id,
            wo.order_number,
            string.Join(", ", wo.products.Select(p => p.product.name)),
            wo.machine.name,
            wo.status.ToString(),
            wo.products.Sum(p => p.target_quantity),
            wo.products.Sum(p => p.produced_quantity),
            wo.created_at
        )).ToList();

        // Machine statuses with current WO
        var machineStatuses = await _db.Machines.AsNoTracking()
            .Where(m => m.is_active)
            .Select(m => new MachineStatusSummary(
                m.id, m.name, m.code, m.status.ToString(),
                m.work_orders
                    .Where(wo => wo.status == WorkOrderStatus.InProgress)
                    .Select(wo => wo.order_number)
                    .FirstOrDefault()))
            .ToListAsync();

        return new DashboardSummaryResponse(
            totalProducts, totalMachines,
            machinesRunning, machinesIdle, machinesMaint, machinesError,
            totalWo, woPending, woInProgress, woCompletedToday,
            todayProduced, todayDefects, todayYieldRate,
            recentWo, machineStatuses);
    }
}
