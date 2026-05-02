using Microsoft.AspNetCore.Mvc;
using RetailCoreEcommerce.Contracts.Models.Product;
using RetailCoreEcommerce.StoreFront.Models;
using RetailCoreEcommerce.StoreFront.Services;
using RetailCoreEcommerce.StoreFront.Services.Category;
using RetailCoreEcommerce.StoreFront.Services.Product;

namespace RetailCoreEcommerce.StoreFront.Controllers;

public class HomeController : BaseController
{
    private readonly IProductApiService _productApiService;

    public HomeController(
        IProductApiService productApiService,
        ICategoryApiService categoryApiService)
        : base(categoryApiService)
    {
        _productApiService = productApiService;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        await PopulateNavCategoriesAsync(ct);

        var categoriesTask = CategoryApiService.GetAllAsync(new()
        {
            IsRootOnly = true,
            PageSize = 8
        }, ct);

        var featuredTask = _productApiService.GetPagedAsync(new GetProductRequest
        {
            IsActive = true,
            PageSize = 8,
            PageNumber = 1
        }, ct);

        await Task.WhenAll(categoriesTask, featuredTask);

        var categoriesResult = categoriesTask.Result;
        var featuredResult = featuredTask.Result;

        if (!categoriesResult.IsSuccess) SetApiError(categoriesResult.Error);
        if (!featuredResult.IsSuccess) SetApiError(featuredResult.Error);

        var viewModel = new HomeViewModel
        {
            Categories = categoriesResult.Data?.Items?.ToList() ?? [],
            FeaturedProducts = featuredResult.Data?.Items?.ToList() ?? []
        };

        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
