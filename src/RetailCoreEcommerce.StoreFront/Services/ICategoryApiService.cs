using RetailCoreEcommerce.Contracts.Models.Category;
using RetailCoreEcommerce.Contracts.Shared;
using RetailCoreEcommerce.StoreFront.Models;

namespace RetailCoreEcommerce.StoreFront.Services;

public interface ICategoryApiService
{
    Task<ApiResponse<PaginationResult<GetPagedCategoryResponse>>> GetAllAsync(
        GetAllCategoriesRequest request,
        CancellationToken ct = default);
}
