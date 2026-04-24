using RetailCoreEcommerce.Contracts.Models.Product;
using RetailCoreEcommerce.Contracts.Shared;

namespace RetailCoreEcommerce.Application.Abstractions;

public interface IProductService
{
    Task<Result> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateProductAsync(UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<GetProductResponse>> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PaginationResult<GetAllProductResponse>>> GetPagedProductsAsync(GetProductRequest request,
        CancellationToken cancellationToken = default);
}