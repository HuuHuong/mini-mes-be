using mini_mes_be.DTOs.Pagination;
using mini_mes_be.DTOs.Products;

namespace mini_mes_be.Services;

public interface IProductService
{
    Task<PaginatedResponse<ProductResponse>> GetAllAsync(PaginatedRequest request);
    Task<ProductDetailResponse> GetByIdAsync(int id);
    Task<ProductResponse> CreateAsync(CreateProductRequest request);
    Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request);
    Task DeleteAsync(int id);
}
