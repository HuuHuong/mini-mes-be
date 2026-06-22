using mini_mes_be.DTOs.Pagination;
using mini_mes_be.DTOs.WorkOrders;
using mini_mes_be.Models.Enums;

namespace mini_mes_be.Services;

public interface IWorkOrderService
{
    Task<PaginatedResponse<WorkOrderResponse>> GetAllAsync(PaginatedRequest request, WorkOrderStatus? statusFilter);
    Task<WorkOrderDetailResponse> GetByIdAsync(int id);
    Task<WorkOrderResponse> CreateAsync(CreateWorkOrderRequest request, int userId);
    Task<WorkOrderResponse> UpdateAsync(int id, UpdateWorkOrderRequest request);
    Task<WorkOrderResponse> UpdateStatusAsync(int id, UpdateWorkOrderStatusRequest request, int userId);
    Task<WorkOrderResponse> RecordOutputAsync(int id, RecordOutputRequest request, int userId);
}
