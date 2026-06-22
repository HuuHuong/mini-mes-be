namespace mini_mes_be.DTOs.Dashboard;

public record DashboardSummaryResponse(
    int total_products,
    int total_machines,
    int machines_running,
    int machines_idle,
    int machines_maintenance,
    int machines_error,
    int total_work_orders,
    int work_orders_pending,
    int work_orders_in_progress,
    int work_orders_completed_today,
    int today_output,
    int today_defects,
    double today_yield_rate,
    IEnumerable<RecentWorkOrderResponse> recent_work_orders,
    IEnumerable<MachineStatusSummary> machine_statuses
);

public record RecentWorkOrderResponse(
    int id,
    string order_number,
    string product_name,
    string machine_name,
    string status,
    int target_quantity,
    int produced_quantity,
    long created_at
);

public record MachineStatusSummary(
    int id,
    string name,
    string code,
    string status,
    string? current_work_order
);
