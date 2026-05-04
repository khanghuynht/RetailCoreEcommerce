using Microsoft.AspNetCore.Mvc;
using RetailCoreEcommerce.Contracts.Models.Order;
using RetailCoreEcommerce.StoreFront.Models;
using RetailCoreEcommerce.StoreFront.Services.Category;
using RetailCoreEcommerce.StoreFront.Services.Checkout;
using RetailCoreEcommerce.StoreFront.Services.Order;
using RetailCoreEcommerce.StoreFront.Services.TokenStorage;

namespace RetailCoreEcommerce.StoreFront.Controllers;

public class CheckoutController(
    ICheckoutApiService checkoutApiService,
    IOrderApiService orderApiService,
    ITokenStorageService tokenStorage,
    ICategoryApiService categoryApiService,
    IConfiguration configuration) : BaseController(categoryApiService)
{
    [HttpGet]
    public IActionResult PublishableKey()
    {
        var key = configuration["Stripe:PublishableKey"] ?? string.Empty;
        return Content(key, "text/plain");
    }

    /// <summary>Stripe redirects here after some payment methods complete 3DS / wallet flows.</summary>
    [HttpGet]
    public async Task<IActionResult> PaymentReturn(CancellationToken ct)
    {
        if (!tokenStorage.IsAuthenticated)
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Payment", "Checkout") });

        await PopulateNavCategoriesAsync(ct);
        return View();
    }

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

    [HttpGet]
    public async Task<IActionResult> Payment(CancellationToken ct)
    {
        if (!tokenStorage.IsAuthenticated)
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Payment", "Checkout") });

        await PopulateNavCategoriesAsync(ct);

        var previewResult = await checkoutApiService.GetPreviewAsync(ct);

        if (!previewResult.IsSuccess || previewResult.Data is null)
        {
            SetApiError(previewResult.Error);
            return RedirectToAction("Index", "Cart");
        }

        if (previewResult.Data.HasIssues)
        {
            TempData["CartError"] = "Please resolve cart issues before proceeding to payment.";
            return RedirectToAction(nameof(Preview));
        }

        if (previewResult.Data.Items.Count == 0)
        {
            TempData["CartError"] = "Your cart is empty.";
            return RedirectToAction("Index", "Cart");
        }

        var model = new PaymentViewModel
        {
            Preview = previewResult.Data
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request, CancellationToken ct)
    {
        if (!tokenStorage.IsAuthenticated)
            return Unauthorized();

        var result = await orderApiService.PlaceOrderAsync(request, ct);

        // Return JSON — this endpoint is called via fetch() from Stripe.js page
        return Json(new
        {
            isSuccess = result.IsSuccess,
            data = result.IsSuccess ? result.Data : null,
            error = result.IsSuccess ? null : result.Error
        });
    }

    [HttpGet]
    public async Task<IActionResult> Success(Guid orderId, string orderCode, CancellationToken ct)
    {
        if (!tokenStorage.IsAuthenticated)
            return RedirectToAction("Login", "Auth");

        await PopulateNavCategoriesAsync(ct);

        ViewBag.OrderId = orderId;
        ViewBag.OrderCode = orderCode;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Failed(Guid orderId, string orderCode, CancellationToken ct)
    {
        if (!tokenStorage.IsAuthenticated)
            return RedirectToAction("Login", "Auth");

        await PopulateNavCategoriesAsync(ct);

        ViewBag.OrderId = orderId;
        ViewBag.OrderCode = orderCode;
        return View();
    }
}
