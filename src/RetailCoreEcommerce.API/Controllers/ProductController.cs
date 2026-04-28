using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Contracts.Models.Inventory;
using RetailCoreEcommerce.Contracts.Models.Product;
using RetailCoreEcommerce.Contracts.Shared;

namespace RetailCoreEcommerce.API.Controllers;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/products")]
public class ProductController : BaseApiController
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _productService.CreateProductAsync(request, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProductById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _productService.GetProductByIdAsync(id, cancellationToken);
        return FromResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] GetProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _productService.GetPagedProductsAsync(request, cancellationToken);
        return FromResult(result);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        // enforce route id as source of truth
        request.Id = id;
        var result = await _productService.UpdateProductAsync(request, cancellationToken);
        return FromResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _productService.DeleteProductAsync(id, cancellationToken);
        return FromResult(result);
    }

    // POST because it adds a new image to gallery 
    [HttpPost("{id:guid}/thumbnail")]
    public async Task<IActionResult> UploadThumbnail(
        Guid id,
        IFormFile file,
        [FromServices] IProductImageService productImageService,
        CancellationToken cancellationToken)
    {
        await using var fileRequest = new FileUploadRequest(
            file.OpenReadStream(),
            file.FileName,
            file.ContentType,
            file.Length);

        var result = await productImageService.UploadThumbnailAsync(id, fileRequest, cancellationToken);
        return FromResult(result);
    }

    [HttpPatch("{id:guid}/inventory")]
    public async Task<IActionResult> UpdateInventoryQuantity(
        Guid id,
        [FromBody] UpdateInventoryQuantityRequest request,
        [FromServices] IInventoryService inventoryService,
        CancellationToken cancellationToken)
    {
        request.ProductId = id;
        var result = await inventoryService.UpdateInventoryQuantityAsync(request, cancellationToken);
        return FromResult(result);
    }
}