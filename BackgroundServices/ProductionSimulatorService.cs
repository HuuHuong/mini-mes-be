using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using mini_mes_be.Data;
using mini_mes_be.Hubs;
using mini_mes_be.Models;
using mini_mes_be.Models.Enums;

namespace mini_mes_be.BackgroundServices;

/// <summary>
/// Background service that simulates production output for active work orders.
/// Every interval, it picks all InProgress work orders and adds a small random output.
/// This is useful for demo/testing without real PLC/SCADA integration.
/// Disable in production by setting "Simulation:Enabled" to false.
/// </summary>
public class ProductionSimulatorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<MesHub> _hub;
    private readonly ILogger<ProductionSimulatorService> _logger;
    private readonly IConfiguration _config;

    public ProductionSimulatorService(
        IServiceScopeFactory scopeFactory,
        IHubContext<MesHub> hub,
        ILogger<ProductionSimulatorService> logger,
        IConfiguration config)
    {
        _scopeFactory = scopeFactory;
        _hub = hub;
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var enabled = _config.GetValue<bool>("Simulation:Enabled", false);
        if (!enabled)
        {
            _logger.LogInformation("Production simulator is DISABLED. Set Simulation:Enabled=true to enable.");
            return;
        }

        var intervalSeconds = _config.GetValue<int>("Simulation:IntervalSeconds", 10);
        _logger.LogInformation("Production simulator started. Interval: {Interval}s", intervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SimulateProductionCycleAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in production simulation cycle");
            }

            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
        }
    }

    private async Task SimulateProductionCycleAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var activeOrders = await db.WorkOrders
            .Include(wo => wo.products).ThenInclude(wop => wop.product)
            .Include(wo => wo.machine)
            .Where(wo => wo.status == WorkOrderStatus.InProgress)
            .ToListAsync();

        if (activeOrders.Count == 0) return;

        var random = new Random();

        foreach (var wo in activeOrders)
        {
            var updatedAny = false;

            foreach (var wop in wo.products)
            {
                var remaining = wop.target_quantity - wop.produced_quantity;
                if (remaining <= 0) continue;

                // Simulate: produce 1-5 units per cycle
                var produced = Math.Min(random.Next(1, 6), remaining);
                // ~5% defect rate
                var defects = random.NextDouble() < 0.05 ? 1 : 0;
                defects = Math.Min(defects, produced);

                wop.produced_quantity += produced;
                wop.defect_quantity += defects;
                updatedAny = true;

                // Log
                db.WorkOrderLogs.Add(new WorkOrderLog
                {
                    work_order_id = wo.id,
                    event_type = "OutputRecorded",
                    message = $"[Simulator] Produced {produced} (defect: {defects}) for {wop.product.name}. Total: {wop.produced_quantity}/{wop.target_quantity}",
                    new_value = $"{wop.produced_quantity}/{wop.target_quantity}"
                });

                // Inventory transaction for good output
                var goodQty = produced - defects;
                if (goodQty > 0)
                {
                    db.InventoryTransactions.Add(new InventoryTransaction
                    {
                        product_id = wop.product_id,
                        type = InventoryTransactionType.Production,
                        quantity = goodQty,
                        work_order_id = wo.id,
                        reference = $"[Simulator] {wo.order_number} ({wop.product.name})",
                        user_id = wo.created_by_user_id
                    });
                }

                // ── SignalR broadcast ──
                await _hub.Clients.All.SendAsync("ProductionOutputUpdated", new
                {
                    work_order_id = wo.id,
                    order_number = wo.order_number,
                    product_id = wop.product_id,
                    product_name = wop.product.name,
                    produced_quantity = wop.produced_quantity,
                    target_quantity = wop.target_quantity,
                    defect_quantity = wop.defect_quantity,
                    status = wo.status.ToString(),
                    is_simulated = true,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });
            }

            if (updatedAny)
            {
                wo.updated_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                // Auto-complete if ALL products reached targets
                if (wo.products.All(p => p.produced_quantity >= p.target_quantity))
                {
                    wo.status = WorkOrderStatus.Completed;
                    wo.actual_end = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                }
            }
        }

        await db.SaveChangesAsync();

        // Broadcast dashboard update
        await _hub.Clients.All.SendAsync("DashboardUpdated", new
        {
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            message = "Production simulation cycle completed"
        });

        _logger.LogDebug("Simulation cycle: processed {Count} active work orders", activeOrders.Count);
    }
}
