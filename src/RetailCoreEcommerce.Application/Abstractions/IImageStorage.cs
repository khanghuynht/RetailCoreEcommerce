using RetailCoreEcommerce.Contracts.Shared;

namespace RetailCoreEcommerce.Application.Abstractions;

public interface IImageStorage
{
    Task<ImageUploadResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string publicId,
        CancellationToken cancellationToken = default);
}