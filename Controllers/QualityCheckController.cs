using Microsoft.AspNetCore.Mvc;
using mini_mes_be.DTOs;
using mini_mes_be.DTOs.Pagination;
using mini_mes_be.DTOs.QualityChecks;
using mini_mes_be.Models.Enums;
using mini_mes_be.Services;
using System.Security.Claims;

namespace mini_mes_be.Controllers;

[ApiController]
[Route("v1/quality-checks")]
public class QualityCheckController : ControllerBase
{
    private readonly IQualityCheckService _service;
    public QualityCheckController(IQualityCheckService service) => _service = service;

    private int UserId =>
        int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value, out var id) ? id : 0;

    // ── GET /v1/quality-checks ────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<QualityCheckResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginatedRequest request,
        [FromQuery] int? work_order_id,
        [FromQuery] QualityCheckResult? result)
    {
        var response = await _service.GetAllAsync(request, work_order_id, result);
        return Ok(ApiResponse<PaginatedResponse<QualityCheckResponse>>.Ok(response));
    }

    // ── POST /v1/quality-checks ───────────────────────────────────────────
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<QualityCheckResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateQualityCheckRequest request)
    {
        var response = await _service.CreateAsync(request, UserId);
        return CreatedAtAction(nameof(GetAll), ApiResponse<QualityCheckResponse>.Ok(response, "Quality check recorded"));
    }
}
