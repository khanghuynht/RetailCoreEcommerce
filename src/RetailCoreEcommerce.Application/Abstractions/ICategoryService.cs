using RetailCoreEcommerce.Contracts.Models.Category;
using RetailCoreEcommerce.Contracts.Models.Product;
using RetailCoreEcommerce.Contracts.Shared;

namespace RetailCoreEcommerce.Application.Abstractions;

public interface ICategoryService
{
    Task<Result> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateCategoryAsync(UpdateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<GetCategoryResponse>> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PaginationResult<GetAllCategoryResponse>>> GetAllCategoriesAsync(GetAllCategoriesRequest request,
        CancellationToken cancellationToken = default);
}