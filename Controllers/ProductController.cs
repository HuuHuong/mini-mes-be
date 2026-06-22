using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mini_mes_be.DTOs;
using mini_mes_be.DTOs.Pagination;
using mini_mes_be.DTOs.Products;
using mini_mes_be.Services;

namespace mini_mes_be.Controllers;

[ApiController]
[Route("v1/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _service;
    public ProductController(IProductService service) => _service = service;

    // ── GET /v1/products ──────────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<ProductResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginatedRequest request)
    {
        var result = await _service.GetAllAsync(request);
        return Ok(ApiResponse<PaginatedResponse<ProductResponse>>.Ok(result));
    }

    // ── GET /v1/products/{id} ─────────────────────────────────────────────
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<ProductDetailResponse>.Ok(result));
    }

    // ── POST /v1/products ─────────────────────────────────────────────────
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.id },
            ApiResponse<ProductResponse>.Ok(result, "Product created"));
    }

    // ── PUT /v1/products/{id} ─────────────────────────────────────────────
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<ProductResponse>.Ok(result, "Product updated"));
    }

    // ── DELETE /v1/products/{id} ──────────────────────────────────────────
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
