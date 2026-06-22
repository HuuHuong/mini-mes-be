using mini_mes_be.DTOs.Pagination;
using mini_mes_be.DTOs.QualityChecks;
using mini_mes_be.Models.Enums;

namespace mini_mes_be.Services;

public interface IQualityCheckService
{
    Task<PaginatedResponse<QualityCheckResponse>> GetAllAsync(PaginatedRequest request, int? workOrderId, QualityCheckResult? resultFilter);
    Task<QualityCheckResponse> CreateAsync(CreateQualityCheckRequest request, int userId);
}
