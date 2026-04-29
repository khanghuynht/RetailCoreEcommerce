using Microsoft.AspNetCore.Mvc;
using RetailCoreEcommerce.Contracts.Models.Category;
using RetailCoreEcommerce.Contracts.Models.Product;
using RetailCoreEcommerce.StoreFront.Models;
using RetailCoreEcommerce.StoreFront.Services;

namespace RetailCoreEcommerce.StoreFront.Controllers;

public class ShopController : BaseController
{
    private readonly IProductApiService _productApiService;

    public ShopController(
        IProductApiService productApiService,
        ICategoryApiService categoryApiService)
        : base(categoryApiService)
    {
        _productApiService = productApiService;
    }

    public async Task<IActionResult> Index(
        string? name,
        Guid? categoryId,
        int page = 1,
        int pageSize = 12,
        CancellationToken ct = default)
    {
        await PopulateNavCategoriesAsync(ct);

        var categoriesTask = CategoryApiService.GetAllAsync(new GetAllCategoriesRequest
        {
            PageSize = 50
        }, ct);

        var productsTask = _productApiService.GetPagedAsync(new GetProductRequest
        {
            IsActive = true,
            Name = name,
            CategoryId = categoryId,
            PageNumber = page,
            PageSize = pageSize
        }, ct);

        await Task.WhenAll(categoriesTask, productsTask);

        var categoriesResult = categoriesTask.Result;
        var productsResult = productsTask.Result;

        if (!categoriesResult.IsSuccess) SetApiError(categoriesResult.Error);
        if (!productsResult.IsSuccess) SetApiError(productsResult.Error);

        var viewModel = new ShopViewModel
        {
            Products = productsResult.Data,
            Categories = categoriesResult.Data?.Items?.ToList() ?? [],
            SearchName = name,
            SelectedCategoryId = categoryId,
            CurrentPage = page,
            PageSize = pageSize
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Detail(Guid id, CancellationToken ct)
    {
        await PopulateNavCategoriesAsync(ct);

        var result = await _productApiService.GetByIdAsync(id, ct);

        if (!result.IsSuccess || result.Data is null)
        {
            SetApiError(result.Error);
            return View("Error", result.Error?.Message ?? "Product not found.");
        }

        return View(new ProductDetailViewModel { Product = result.Data });
    }
}
