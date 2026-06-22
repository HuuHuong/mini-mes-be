using Microsoft.AspNetCore.Mvc;
using mini_mes_be.DTOs;
using mini_mes_be.DTOs.Machines;
using mini_mes_be.DTOs.Pagination;
using mini_mes_be.Models.Enums;
using mini_mes_be.Services;

namespace mini_mes_be.Controllers;

[ApiController]
[Route("v1/machines")]
public class MachineController : ControllerBase
{
    private readonly IMachineService _service;
    public MachineController(IMachineService service) => _service = service;

    // ── GET /v1/machines ──────────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<MachineResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginatedRequest request, [FromQuery] MachineStatus? status)
    {
        var result = await _service.GetAllAsync(request, status);
        return Ok(ApiResponse<PaginatedResponse<MachineResponse>>.Ok(result));
    }

    // ── GET /v1/machines/{id} ─────────────────────────────────────────────
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<MachineDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<MachineDetailResponse>.Ok(result));
    }

    // ── POST /v1/machines ─────────────────────────────────────────────────
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<MachineResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateMachineRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.id },
            ApiResponse<MachineResponse>.Ok(result, "Machine created"));
    }

    // ── PUT /v1/machines/{id} ─────────────────────────────────────────────
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<MachineResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMachineRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<MachineResponse>.Ok(result, "Machine updated"));
    }

    // ── PATCH /v1/machines/{id}/status ────────────────────────────────────
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<MachineResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateMachineStatusRequest request)
    {
        var result = await _service.UpdateStatusAsync(id, request);
        return Ok(ApiResponse<MachineResponse>.Ok(result, "Machine status updated"));
    }
}
