using Microsoft.AspNetCore.Mvc;
using RetailCoreEcommerce.Contracts.Models.Category;
using RetailCoreEcommerce.StoreFront.Models;
using RetailCoreEcommerce.StoreFront.Services;
using RetailCoreEcommerce.StoreFront.Services.Category;

namespace RetailCoreEcommerce.StoreFront.Controllers;

public abstract class BaseController : Controller
{
    protected readonly ICategoryApiService CategoryApiService;

    protected BaseController(ICategoryApiService categoryApiService)
    {
        CategoryApiService = categoryApiService;
    }

    /// <summary>
    /// Surfaces an API error message into ViewBag so views can render it.
    /// </summary>
    protected void SetApiError(ApiError? error)
    {
        if (error is not null)
            ViewBag.ApiError = $"[{error.Code}] {error.Message}";
    }

    /// <summary>
    /// Loads top-level categories into ViewBag so _Layout can render the nav menu,
    /// and exposes the current user's auth state for the navbar.
    /// </summary>
    protected async Task PopulateNavCategoriesAsync(CancellationToken ct = default)
    {
        var result = await CategoryApiService.GetAllAsync(new GetAllCategoriesRequest
        {
            PageSize = 50
        }, ct);

        ViewBag.NavCategories = result.Data?.Items?.ToList() ?? [];

        // Expose auth state — reads from the HTTP-only cookie key set by CookieTokenStorageService
        ViewBag.IsAuthenticated = !string.IsNullOrEmpty(Request.Cookies["rc_access"]);
        ViewBag.UserFullName = Request.Cookies["rc_user"];
    }
}
