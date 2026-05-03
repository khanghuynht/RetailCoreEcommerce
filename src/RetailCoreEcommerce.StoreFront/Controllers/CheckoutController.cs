using Microsoft.AspNetCore.Mvc;
using RetailCoreEcommerce.StoreFront.Services.Category;
using RetailCoreEcommerce.StoreFront.Services.Checkout;
using RetailCoreEcommerce.StoreFront.Services.TokenStorage;

namespace RetailCoreEcommerce.StoreFront.Controllers;

public class CheckoutController(
    ICheckoutApiService checkoutApiService,
    ITokenStorageService tokenStorage,
    ICategoryApiService categoryApiService) : BaseController(categoryApiService)
{
    [HttpGet]
    public async Task<IActionResult> Preview(CancellationToken ct)
    {
        if (!tokenStorage.IsAuthenticated)
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Preview", "Checkout") });

        await PopulateNavCategoriesAsync(ct);

        var result = await checkoutApiService.GetPreviewAsync(ct);

        if (!result.IsSuccess)
        {
            SetApiError(result.Error);
            return RedirectToAction("Index", "Cart");
        }

        return View(result.Data);
    }
}
