using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Contracts.Models.Category;

namespace RetailCoreEcommerce.API.Controllers;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/categories")]
public class CategoryController : BaseApiController
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _categoryService.CreateCategoryAsync(request, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCategoryById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id, cancellationToken);
        return FromResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetCategoryPaged(
        [FromQuery] GetAllCategoriesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetPagedCategoriesAsync(request, cancellationToken);
        return FromResult(result);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> PatchCategory(
        Guid id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        // enforce route id as source of truth
        request.Id = id;

        var result = await _categoryService.UpdateCategoryAsync(request, cancellationToken);
        return FromResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _categoryService.DeleteCategoryAsync(id, cancellationToken);
        return FromResult(result);
    }
}