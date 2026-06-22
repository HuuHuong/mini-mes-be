using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mini_mes_be.DTOs;
using mini_mes_be.DTOs.Pagination;
using mini_mes_be.DTOs.WorkOrders;
using mini_mes_be.Models.Enums;
using mini_mes_be.Services;
using System.Security.Claims;

namespace mini_mes_be.Controllers;

[ApiController]
[Route("v1/work-orders")]
public class WorkOrderController : ControllerBase
{
    private readonly IWorkOrderService _service;
    public WorkOrderController(IWorkOrderService service) => _service = service;

    private int UserId =>
        int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value, out var id) ? id : 0;

    // ── GET /v1/work-orders ───────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<WorkOrderResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginatedRequest request, [FromQuery] WorkOrderStatus? status)
    {
        var result = await _service.GetAllAsync(request, status);
        return Ok(ApiResponse<PaginatedResponse<WorkOrderResponse>>.Ok(result));
    }

    // ── GET /v1/work-orders/{id} ──────────────────────────────────────────
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<WorkOrderDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<WorkOrderDetailResponse>.Ok(result));
    }

    // ── POST /v1/work-orders ──────────────────────────────────────────────
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<WorkOrderResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateWorkOrderRequest request)
    {
        var result = await _service.CreateAsync(request, UserId);
        return CreatedAtAction(nameof(GetById), new { id = result.id },
            ApiResponse<WorkOrderResponse>.Ok(result, "Work order created"));
    }

    // ── PUT /v1/work-orders/{id} ──────────────────────────────────────────
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<WorkOrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkOrderRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<WorkOrderResponse>.Ok(result, "Work order updated"));
    }

    // ── PATCH /v1/work-orders/{id}/status ─────────────────────────────────
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<WorkOrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateWorkOrderStatusRequest request)
    {
        var result = await _service.UpdateStatusAsync(id, request, UserId);
        return Ok(ApiResponse<WorkOrderResponse>.Ok(result, "Work order status updated"));
    }

    // ── POST /v1/work-orders/{id}/output ──────────────────────────────────
    [HttpPost("{id:int}/output")]
    [ProducesResponseType(typeof(ApiResponse<WorkOrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RecordOutput(int id, [FromBody] RecordOutputRequest request)
    {
        var result = await _service.RecordOutputAsync(id, request, UserId);
        return Ok(ApiResponse<WorkOrderResponse>.Ok(result, "Production output recorded"));
    }
}
