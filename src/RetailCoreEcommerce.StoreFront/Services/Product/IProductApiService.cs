using RetailCoreEcommerce.Contracts.Models.Product;
using RetailCoreEcommerce.Contracts.Shared;
using RetailCoreEcommerce.StoreFront.Models;

namespace RetailCoreEcommerce.StoreFront.Services.Product;

public interface IProductApiService
{
    Task<ApiResponse<PaginationResult<GetAllProductResponse>>> GetPagedAsync(
        GetProductRequest request,
        CancellationToken ct = default);

    Task<ApiResponse<GetProductResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
}
