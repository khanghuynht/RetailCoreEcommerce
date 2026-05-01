using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Contracts.Models.File;
using RetailCoreEcommerce.Contracts.Shared;
using RetailCoreEcommerce.Domain;

namespace RetailCoreEcommerce.Application.Services;

public class ProductImageService : IProductImageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorage _imageStorage;

    public ProductImageService(IUnitOfWork unitOfWork, IImageStorage imageStorage)
    {
        _unitOfWork = unitOfWork;
        _imageStorage = imageStorage;
    }

    public async Task<Result> UploadThumbnailAsync(
        Guid productId, FileUploadRequest file,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var productRepo = _unitOfWork.GetRepository<Product, Guid>();

            var product = await productRepo.FindByIdAsync(
                productId, tracking: true, cancellationToken: cancellationToken);

            if (product is null)
                return Result.Failure(new Error("Product.NotFound",
                    $"Product with ID {productId} not found."));

            // Upload new thumbnail to Cloudinary
            var uploadResult = await _imageStorage.UploadAsync(
                file.Content, file.FileName, file.ContentType, cancellationToken);

            // Replace thumbnail url on product
            product.ThumbnailUrl = uploadResult.Url;

            productRepo.Update(product);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ProductService.UploadThumbnailAsync",
                $"Error uploading thumbnail: {ex.Message}"));
        }
    }

    public async Task<Result> AddProductImageAsync(
        Guid productId, FileUploadRequest file, CancellationToken cancellationToken = default)
    {
        try
        {
            var productRepo = _unitOfWork.GetRepository<Product, Guid>();
            var imageRepo = _unitOfWork.GetRepository<ProductImage, Guid>();

            var product = await productRepo.AnyAsync(
                x => x.Id == productId, cancellationToken: cancellationToken);

            if (product is false)
                return Result.Failure(new Error("Product.NotFound",
                    $"Product with ID {productId} not found."));

            // Get current highest position to append new image at end
            var maxPosition = await imageRepo.CountAsync(
                x => x.ProductId == productId, cancellationToken);

            var uploadResult = await _imageStorage.UploadAsync(
                file.Content, file.FileName, file.ContentType, cancellationToken);

            var image = new ProductImage
            {
                ProductId = productId,
                Name = file.FileName,
                ImageUrl = uploadResult.Url,
                PublicId = uploadResult.PublicId,
                Position = maxPosition + 1
            };

            imageRepo.Add(image);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ProductService.AddProductImageAsync",
                $"Error adding product image: {ex.Message}"));
        }
    }

    public async Task<Result> DeleteProductImageAsync(
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var imageRepo = _unitOfWork.GetRepository<ProductImage, Guid>();

            var image = await imageRepo.FindByIdAsync(
                imageId, tracking: true, cancellationToken: cancellationToken);

            if (image is null)
                return Result.Failure(new Error("ProductImage.NotFound",
                    $"Image with ID {imageId} not found."));

            // Delete from Cloudinary first
            await _imageStorage.DeleteAsync(image.PublicId, cancellationToken);

            // Then remove from DB
            imageRepo.Delete(image);

            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ProductService.DeleteProductImageAsync",
                $"Error deleting product image: {ex.Message}"));
        }
    }

    public async Task<Result> ReorderProductImagesAsync(
        Guid productId, List<Guid> orderedImageIds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var imageRepo = _unitOfWork.GetRepository<ProductImage, Guid>();

            var images = imageRepo.FindAll(
                x => x.ProductId == productId, tracking: true);

            var imageList = images.ToList();

            // Validate all provided ids belong to this product
            if (orderedImageIds.Count != imageList.Count ||
                orderedImageIds.Any(id => imageList.All(i => i.Id != id)))
                return Result.Failure(new Error("ProductImage.InvalidReorder",
                    "Provided image ids do not match product images."));

            // Assign new positions based on order of provided ids
            for (var i = 0; i < orderedImageIds.Count; i++)
            {
                // Find image by id and update its position (1-based index)
                var image = imageList.First(x => x.Id == orderedImageIds[i]);
                image.Position = i + 1;
            }

            imageRepo.UpdateRange(imageList);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ProductService.ReorderProductImagesAsync",
                $"Error reordering product images: {ex.Message}"));
        }
    }
}