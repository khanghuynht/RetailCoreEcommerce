using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Contracts.Constants;
using RetailCoreEcommerce.Contracts.Models.File;
using RetailCoreEcommerce.Contracts.Shared;

namespace RetailCoreEcommerce.API.Controllers;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/products")]
public class ProductImageController : BaseApiController
{
    private readonly IProductImageService _productImageService;

    public ProductImageController(IProductImageService productImageService)
    {
        _productImageService = productImageService;
    }

    // POST because it adds a new image to gallery
    [HttpPost("{id:guid}/images")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> AddProductImage(
        Guid id,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var fileRequest = new FileUploadRequest(
            file.OpenReadStream(),
            file.FileName,
            file.ContentType,
            file.Length);

        var result = await _productImageService.AddProductImageAsync(id, fileRequest, cancellationToken);
        return FromResult(result);
    }

    // DELETE a specific image from gallery by imageId
    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> DeleteProductImage(
        Guid imageId,
        CancellationToken cancellationToken)
    {
        var result = await _productImageService.DeleteProductImageAsync(imageId, cancellationToken);
        return FromResult(result);
    }

    // PATCH to reorder images by providing ordered list of image ids
    [HttpPatch("{id:guid}/images/reorder")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> ReorderProductImages(
        Guid id,
        [FromBody] List<Guid> orderedImageIds,
        CancellationToken cancellationToken)
    {
        var result = await _productImageService.ReorderProductImagesAsync(
            id, orderedImageIds, cancellationToken);

        return FromResult(result);
    }
    
}