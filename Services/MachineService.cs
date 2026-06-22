using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using mini_mes_be.Data;
using mini_mes_be.DTOs.Machines;
using mini_mes_be.DTOs.Pagination;
using mini_mes_be.Hubs;
using mini_mes_be.Models;
using mini_mes_be.Models.Enums;

namespace mini_mes_be.Services;

public class MachineService : IMachineService
{
    private readonly AppDbContext _db;
    private readonly IHubContext<MesHub> _hub;

    public MachineService(AppDbContext db, IHubContext<MesHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    public async Task<PaginatedResponse<MachineResponse>> GetAllAsync(PaginatedRequest request, MachineStatus? statusFilter)
    {
        var query = _db.Machines.AsNoTracking().AsQueryable();

        if (statusFilter.HasValue)
            query = query.Where(m => m.status == statusFilter.Value);

        if (!string.IsNullOrWhiteSpace(request.search))
        {
            var search = request.search.Trim().ToLower();
            query = query.Where(m =>
                m.name.ToLower().Contains(search) ||
                m.code.ToLower().Contains(search));
        }

        query = request.sort_by?.ToLower() switch
        {
            "name" => request.sort_direction == "desc" ? query.OrderByDescending(m => m.name) : query.OrderBy(m => m.name),
            "code" => request.sort_direction == "desc" ? query.OrderByDescending(m => m.code) : query.OrderBy(m => m.code),
            "status" => request.sort_direction == "desc" ? query.OrderByDescending(m => m.status) : query.OrderBy(m => m.status),
            _ => query.OrderBy(m => m.name)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((request.page - 1) * request.page_size)
            .Take(request.page_size)
            .Select(m => new MachineResponse(
                m.id, m.name, m.code, m.location,
                m.status.ToString(), m.is_active,
                m.created_at, m.updated_at))
            .ToListAsync();

        return PaginatedResponse<MachineResponse>.Create(items, totalCount, request.page, request.page_size);
    }

    public async Task<MachineDetailResponse> GetByIdAsync(int id)
    {
        var machine = await _db.Machines.AsNoTracking().FirstOrDefaultAsync(m => m.id == id)
            ?? throw new KeyNotFoundException($"Machine with ID {id} not found.");

        var activeWoCount = await _db.WorkOrders
            .CountAsync(wo => wo.machine_id == id &&
                (wo.status == WorkOrderStatus.Pending || wo.status == WorkOrderStatus.InProgress));

        return new MachineDetailResponse(
            machine.id, machine.name, machine.code, machine.location,
            machine.status.ToString(), machine.is_active,
            activeWoCount, machine.created_at, machine.updated_at);
    }

    public async Task<MachineResponse> CreateAsync(CreateMachineRequest request)
    {
        if (await _db.Machines.AnyAsync(m => m.code == request.code))
            throw new InvalidOperationException($"A machine with code '{request.code}' already exists.");

        var machine = new Machine
        {
            name = request.name,
            code = request.code,
            location = request.location,
            status = MachineStatus.Idle
        };

        _db.Machines.Add(machine);
        await _db.SaveChangesAsync();

        return new MachineResponse(
            machine.id, machine.name, machine.code, machine.location,
            machine.status.ToString(), machine.is_active,
            machine.created_at, machine.updated_at);
    }

    public async Task<MachineResponse> UpdateAsync(int id, UpdateMachineRequest request)
    {
        var machine = await _db.Machines.FindAsync(id)
            ?? throw new KeyNotFoundException($"Machine with ID {id} not found.");

        if (await _db.Machines.AnyAsync(m => m.code == request.code && m.id != id))
            throw new InvalidOperationException($"A machine with code '{request.code}' already exists.");

        machine.name = request.name;
        machine.code = request.code;
        machine.location = request.location;
        machine.is_active = request.is_active;
        machine.updated_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await _db.SaveChangesAsync();

        return new MachineResponse(
            machine.id, machine.name, machine.code, machine.location,
            machine.status.ToString(), machine.is_active,
            machine.created_at, machine.updated_at);
    }

    public async Task<MachineResponse> UpdateStatusAsync(int id, UpdateMachineStatusRequest request)
    {
        var machine = await _db.Machines.FindAsync(id)
            ?? throw new KeyNotFoundException($"Machine with ID {id} not found.");

        var oldStatus = machine.status;
        machine.status = request.status;
        machine.updated_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await _db.SaveChangesAsync();

        // ── SignalR: broadcast machine status change ──
        var response = new MachineResponse(
            machine.id, machine.name, machine.code, machine.location,
            machine.status.ToString(), machine.is_active,
            machine.created_at, machine.updated_at);

        await _hub.Clients.All.SendAsync("MachineStatusChanged", new
        {
            machine_id = machine.id,
            machine_name = machine.name,
            old_status = oldStatus.ToString(),
            new_status = machine.status.ToString(),
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        });

        return response;
    }
}
