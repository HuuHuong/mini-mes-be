using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using mini_mes_be.Data;
using mini_mes_be.Models.Enums;

namespace mini_mes_be.Hubs;

/// <summary>
/// Central SignalR hub for real-time MES events.
/// Requires a valid JWT token passed via query string: ?access_token=xxx
///
/// On connect, the hub automatically:
/// 1. Extracts userId from the verified JWT claims
/// 2. Joins the user into group "User_{userId}"
/// 3. Joins the user into group "Dashboard" (global KPI updates)
/// 4. Looks up machines the user manages (via active work orders) and auto-subscribes
///
/// FE connects with:
///   new HubConnectionBuilder()
///     .withUrl("http://localhost:5130/mes-hub", { accessTokenFactory: () => token })
///     .build();
/// </summary>
[Authorize]
public class MesHub : Hub
{
    private readonly ILogger<MesHub> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public MesHub(ILogger<MesHub> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    /// <summary>Extracts the user ID (int) from the JWT "sub" claim.</summary>
    private int? GetUserId()
    {
        var sub = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? Context.User?.FindFirst("sub")?.Value;
        return int.TryParse(sub, out var id) ? id : null;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId == null)
        {
            _logger.LogWarning("Client {ConnectionId} connected but userId could not be resolved from token", Context.ConnectionId);
            Context.Abort();
            return;
        }

        _logger.LogInformation("Client connected: {ConnectionId} (UserId: {UserId})", Context.ConnectionId, userId);

        // ── Join user-specific group ──────────────────────────────────────────
        await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");

        // ── Join global Dashboard group ───────────────────────────────────────
        await Groups.AddToGroupAsync(Context.ConnectionId, "Dashboard");

        // ── Auto-subscribe to machines the user manages ───────────────────────
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Find distinct machine IDs from active work orders created by this user
        var managedMachineIds = await db.WorkOrders
            .Where(wo => wo.created_by_user_id == userId.Value
                && (wo.status == WorkOrderStatus.Pending || wo.status == WorkOrderStatus.InProgress))
            .Select(wo => wo.machine_id)
            .Distinct()
            .ToListAsync();

        foreach (var machineId in managedMachineIds)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Machine_{machineId}");
            _logger.LogDebug("Auto-subscribed User {UserId} to Machine_{MachineId}", userId, machineId);
        }

        // Notify the client which machines they were auto-subscribed to
        await Clients.Caller.SendAsync("Connected", new
        {
            user_id = userId.Value,
            connection_id = Context.ConnectionId,
            subscribed_machines = managedMachineIds,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        });

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        _logger.LogInformation("Client disconnected: {ConnectionId} (UserId: {UserId})", Context.ConnectionId, userId);
        await base.OnDisconnectedAsync(exception);
    }

    // ── Manual subscription methods ──────────────────────────────────────────

    /// <summary>
    /// Subscribe to a specific machine's real-time updates.
    /// </summary>
    public async Task SubscribeMachine(int machineId)
    {
        var userId = GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Machine_{machineId}");
        _logger.LogInformation("User {UserId} subscribed to Machine_{MachineId}", userId, machineId);
        await Clients.Caller.SendAsync("SubscribedMachine", new { machine_id = machineId });
    }

    /// <summary>
    /// Unsubscribe from a specific machine's updates.
    /// </summary>
    public async Task UnsubscribeMachine(int machineId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Machine_{machineId}");
        await Clients.Caller.SendAsync("UnsubscribedMachine", new { machine_id = machineId });
    }

    /// <summary>
    /// Subscribe to a specific work order's real-time updates.
    /// </summary>
    public async Task SubscribeWorkOrder(int workOrderId)
    {
        var userId = GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, $"WorkOrder_{workOrderId}");
        _logger.LogInformation("User {UserId} subscribed to WorkOrder_{WorkOrderId}", userId, workOrderId);
        await Clients.Caller.SendAsync("SubscribedWorkOrder", new { work_order_id = workOrderId });
    }

    /// <summary>
    /// Unsubscribe from a specific work order's updates.
    /// </summary>
    public async Task UnsubscribeWorkOrder(int workOrderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"WorkOrder_{workOrderId}");
        await Clients.Caller.SendAsync("UnsubscribedWorkOrder", new { work_order_id = workOrderId });
    }
}
