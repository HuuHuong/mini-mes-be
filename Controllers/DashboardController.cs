using Microsoft.AspNetCore.Mvc;
using mini_mes_be.DTOs;
using mini_mes_be.DTOs.Dashboard;
using mini_mes_be.Services;

namespace mini_mes_be.Controllers;

[ApiController]
[Route("v1/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;
    public DashboardController(IDashboardService service) => _service = service;

    // ── GET /v1/dashboard/summary ─────────────────────────────────────────
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _service.GetSummaryAsync();
        return Ok(ApiResponse<DashboardSummaryResponse>.Ok(result));
    }
}
