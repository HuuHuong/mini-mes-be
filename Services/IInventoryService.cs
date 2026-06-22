using mini_mes_be.DTOs.Inventory;
using mini_mes_be.DTOs.Pagination;

namespace mini_mes_be.Services;

public interface IInventoryService
{
    Task<PaginatedResponse<InventoryStockResponse>> GetStockSummaryAsync(PaginatedRequest request);
    Task<PaginatedResponse<InventoryTransactionResponse>> GetTransactionsAsync(PaginatedRequest request, int? productId);
    Task<InventoryTransactionResponse> CreateTransactionAsync(CreateInventoryTransactionRequest request, int userId);
}
