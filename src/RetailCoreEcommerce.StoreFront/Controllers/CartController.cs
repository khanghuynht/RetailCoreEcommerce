using Microsoft.AspNetCore.Mvc;
using CartModel = RetailCoreEcommerce.Contracts.Models.Cart.Cart;
using RetailCoreEcommerce.Contracts.Models.Cart;
using RetailCoreEcommerce.StoreFront.Services.Cart;
using RetailCoreEcommerce.StoreFront.Services.Category;
using RetailCoreEcommerce.StoreFront.Services.TokenStorage;


namespace RetailCoreEcommerce.StoreFront.Controllers;

public class CartController(
    ICartApiService cartApiService,
    ITokenStorageService tokenStorage,
    ICategoryApiService categoryApiService) : BaseController(categoryApiService)
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (!tokenStorage.IsAuthenticated)
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Index", "Cart") });

        await PopulateNavCategoriesAsync(ct);

        var result = await cartApiService.GetCartAsync(ct);

        if (!result.IsSuccess)
        {
            SetApiError(result.Error);
            return View(new CartModel());
        }

        return View(result.Data ?? new CartModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(Guid productId, int quantity, string? returnUrl, CancellationToken ct)
    {
        if (!tokenStorage.IsAuthenticated)
            return RedirectToAction("Login", "Auth", new { returnUrl = returnUrl ?? Url.Action("Index", "Cart") });

        if (quantity < 1) quantity = 1;

        var result = await cartApiService.AddItemAsync(new AddCartItemRequest
        {
            ProductId = productId,
            Quantity = quantity
        }, ct);

        if (!result.IsSuccess)
            TempData["CartError"] = result.Error?.Message ?? "Could not add item to cart.";
        else
            TempData["CartSuccess"] = "Item added to cart.";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateItem(Guid productId, int quantity, CancellationToken ct)
    {
        if (!tokenStorage.IsAuthenticated)
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Index", "Cart") });

        if (quantity < 1) quantity = 1;

        var result = await cartApiService.UpdateItemAsync(new UpdateCartItemRequest
        {
            ProductId = productId,
            Quantity = quantity
        }, ct);

        if (!result.IsSuccess)
            TempData["CartError"] = result.Error?.Message ?? "Could not update item.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(Guid productId, CancellationToken ct)
    {
        if (!tokenStorage.IsAuthenticated)
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Index", "Cart") });

        var result = await cartApiService.RemoveItemAsync(productId, ct);

        if (!result.IsSuccess)
            TempData["CartError"] = result.Error?.Message ?? "Could not remove item.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear(CancellationToken ct)
    {
        if (!tokenStorage.IsAuthenticated)
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Index", "Cart") });

        await cartApiService.ClearCartAsync(ct);
        return RedirectToAction(nameof(Index));
    }

}
