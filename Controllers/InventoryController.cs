using Microsoft.AspNetCore.Mvc;
using mini_mes_be.DTOs;
using mini_mes_be.DTOs.Inventory;
using mini_mes_be.DTOs.Pagination;
using mini_mes_be.Services;
using System.Security.Claims;

namespace mini_mes_be.Controllers;

[ApiController]
[Route("v1/inventory")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _service;
    public InventoryController(IInventoryService service) => _service = service;

    private int UserId =>
        int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value, out var id) ? id : 0;

    // ── GET /v1/inventory ─────────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<InventoryStockResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStockSummary([FromQuery] PaginatedRequest request)
    {
        var result = await _service.GetStockSummaryAsync(request);
        return Ok(ApiResponse<PaginatedResponse<InventoryStockResponse>>.Ok(result));
    }

    // ── GET /v1/inventory/transactions ────────────────────────────────────
    [HttpGet("transactions")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<InventoryTransactionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions([FromQuery] PaginatedRequest request, [FromQuery] int? product_id)
    {
        var result = await _service.GetTransactionsAsync(request, product_id);
        return Ok(ApiResponse<PaginatedResponse<InventoryTransactionResponse>>.Ok(result));
    }

    // ── POST /v1/inventory/transactions ───────────────────────────────────
    [HttpPost("transactions")]
    [ProducesResponseType(typeof(ApiResponse<InventoryTransactionResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateInventoryTransactionRequest request)
    {
        var result = await _service.CreateTransactionAsync(request, UserId);
        return CreatedAtAction(nameof(GetTransactions), ApiResponse<InventoryTransactionResponse>.Ok(result, "Inventory transaction recorded"));
    }
}
