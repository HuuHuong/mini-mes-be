using mini_mes_be.DTOs.Machines;
using mini_mes_be.DTOs.Pagination;
using mini_mes_be.Models.Enums;

namespace mini_mes_be.Services;

public interface IMachineService
{
    Task<PaginatedResponse<MachineResponse>> GetAllAsync(PaginatedRequest request, MachineStatus? statusFilter);
    Task<MachineDetailResponse> GetByIdAsync(int id);
    Task<MachineResponse> CreateAsync(CreateMachineRequest request);
    Task<MachineResponse> UpdateAsync(int id, UpdateMachineRequest request);
    Task<MachineResponse> UpdateStatusAsync(int id, UpdateMachineStatusRequest request);
}
