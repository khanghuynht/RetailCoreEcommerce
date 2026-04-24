using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Contracts.Settings;
using ImageUploadResult = RetailCoreEcommerce.Contracts.Shared.ImageUploadResult;

namespace RetailCoreEcommerce.Infrastructure.Cloudinary;

public class CloudinaryImageStorage : IImageStorage
{
    private readonly ICloudinary _cloudinary;
    private readonly string _folder;

    public CloudinaryImageStorage(
        ICloudinary cloudinary,
        IOptions<CloudinarySettings> options)
    {
        _cloudinary = cloudinary;
        _folder = options.Value.Folder;
    }

    public async Task<ImageUploadResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        // Tell cancellation token to stop if request is cancelled
        cancellationToken.ThrowIfCancellationRequested();

        // Configure what and how to upload to Cloudinary
        var uploadParams = new ImageUploadParams
        {
            // Wrap stream into Cloudinary's file descriptor
            File = new FileDescription(fileName, fileStream),

            // Store in the configured folder (e.g. "retailcore/products")
            Folder = _folder,

            // Use original filename as public id base
            UseFilename = true,

            // Append unique suffix to avoid overwriting existing files
            UniqueFilename = true,

            // Do not overwrite if file with same name exists
            Overwrite = false
        };

        // Send upload request to Cloudinary
        var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        // If Cloudinary returned an error, throw with message
        if (result.Error is not null)
            throw new InvalidOperationException($"Cloudinary upload failed: {result.Error.Message}");

        // Map Cloudinary response to our own result type
        return new ImageUploadResult(
            Url: result.SecureUrl.ToString(), // https URL of uploaded image
            PublicId: result.PublicId, // unique id used to reference/delete later
            Width: result.Width,
            Height: result.Height,
            Bytes: result.Bytes);
    }

    public async Task<bool> DeleteAsync(
        string publicId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Tell Cloudinary which image to delete using its PublicId
        var deleteParams = new DeletionParams(publicId);

        var result = await _cloudinary.DestroyAsync(deleteParams);

        // Cloudinary returns "ok" string when deletion succeeds
        return result.Result == "ok";
    }
}