using Microsoft.AspNetCore.Mvc;
using mini_mes_be.DTOs;
using mini_mes_be.DTOs.Bom;
using mini_mes_be.Services;

namespace mini_mes_be.Controllers;

[ApiController]
[Route("v1/products/{productId:int}/bom")]
public class BomController : ControllerBase
{
    private readonly IBomService _service;
    public BomController(IBomService service) => _service = service;

    // ── GET /v1/products/{productId}/bom ───────────────────────────────────
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<BomResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBom(int productId)
    {
        var result = await _service.GetByProductIdAsync(productId);
        return Ok(ApiResponse<BomResponse>.Ok(result));
    }

    // ── POST /v1/products/{productId}/bom/items ───────────────────────────
    [HttpPost("items")]
    [ProducesResponseType(typeof(ApiResponse<BomItemResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddItem(int productId, [FromBody] CreateBomItemRequest request)
    {
        var result = await _service.AddItemAsync(productId, request);
        return CreatedAtAction(nameof(GetBom), new { productId },
            ApiResponse<BomItemResponse>.Ok(result, "BOM item added"));
    }

    // ── PUT /v1/products/{productId}/bom/items/{id} ───────────────────────
    [HttpPut("items/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<BomItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateItem(int productId, int id, [FromBody] UpdateBomItemRequest request)
    {
        var result = await _service.UpdateItemAsync(productId, id, request);
        return Ok(ApiResponse<BomItemResponse>.Ok(result, "BOM item updated"));
    }

    // ── DELETE /v1/products/{productId}/bom/items/{id} ────────────────────
    [HttpDelete("items/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteItem(int productId, int id)
    {
        await _service.DeleteItemAsync(productId, id);
        return NoContent();
    }

    // ── PUT /v1/products/{productId}/bom ──────────────────────────────────
    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<BomResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetBom(int productId, [FromBody] SetBomRequest request)
    {
        var result = await _service.SetBomAsync(productId, request);
        return Ok(ApiResponse<BomResponse>.Ok(result, "BOM updated"));
    }
}
