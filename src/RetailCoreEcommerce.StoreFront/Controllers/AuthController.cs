using Microsoft.AspNetCore.Mvc;
using RetailCoreEcommerce.Contracts.Constants;
using RetailCoreEcommerce.Contracts.Models.Auth;
using RetailCoreEcommerce.StoreFront.Models.Auth;
using RetailCoreEcommerce.StoreFront.Services.Auth;
using RetailCoreEcommerce.StoreFront.Services.Category;
using RetailCoreEcommerce.StoreFront.Services.TokenStorage;

namespace RetailCoreEcommerce.StoreFront.Controllers;

public class AuthController(
    IAuthApiService authApiService,
    ITokenStorageService tokenStorage,
    ICategoryApiService categoryApiService) : BaseController(categoryApiService)
{
    [HttpGet]
    public async Task<IActionResult> Login(string? returnUrl = null, CancellationToken ct = default)
    {
        await PopulateNavCategoriesAsync(ct);
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
    {
        await PopulateNavCategoriesAsync(ct);

        if (!ModelState.IsValid)
            return View(model);

        var result = await authApiService.LoginAsync(new LoginRequest
        {
            LoginIdentifier = model.Email,
            Password = model.Password
        }, ct);

        if (!result.IsSuccess || result.Data is null || result.Data.Role == nameof(UserRole.Admin))
        {
            ModelState.AddModelError(string.Empty, result.Error?.Message ?? "Invalid email or password.");
            return View(model);
        }

        tokenStorage.StoreSession(result.Data.AccessToken, result.Data.RefreshToken, result.Data.FullName);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Register(CancellationToken ct)
    {
        await PopulateNavCategoriesAsync(ct);
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken ct)
    {
        await PopulateNavCategoriesAsync(ct);

        if (!ModelState.IsValid)
            return View(model);

        var result = await authApiService.RegisterAsync(new RegisterRequest
        {
            Email = model.Email,
            Username = model.Username,
            Password = model.Password,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber ?? string.Empty
        }, ct);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error?.Message ?? "Registration failed. Please try again.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Account created! You can now sign in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var accessToken = tokenStorage.GetAccessToken();
        var refreshToken = tokenStorage.GetRefreshToken();

        if (accessToken is not null && refreshToken is not null)
        {
            await authApiService.LogoutAsync(new RefreshTokenRequest
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            }, ct);
        }

        tokenStorage.ClearSession();
        return RedirectToAction("Index", "Home");
    }
}