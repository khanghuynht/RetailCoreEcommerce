using RetailCoreEcommerce.Contracts.Shared;

namespace RetailCoreEcommerce.Application.Abstractions;

public interface IProductImageService
{
    Task<Result> UploadThumbnailAsync(Guid productId, FileUploadRequest file,
        CancellationToken cancellationToken = default);

    Task<Result> AddProductImageAsync(Guid productId, FileUploadRequest file,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteProductImageAsync(Guid imageId, CancellationToken cancellationToken = default);

    Task<Result> ReorderProductImagesAsync(Guid productId, List<Guid> orderedImageIds,
        CancellationToken cancellationToken = default);
}