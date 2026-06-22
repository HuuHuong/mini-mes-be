using mini_mes_be.DTOs.Dashboard;

namespace mini_mes_be.Services;

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummaryAsync();
}
