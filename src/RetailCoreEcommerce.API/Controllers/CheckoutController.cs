using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using RetailCoreEcommerce.Application.Abstractions;

namespace RetailCoreEcommerce.API.Controllers;

public class CheckoutController : BaseApiController
{
    private readonly ICheckoutService _checkoutService;

    public CheckoutController(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    [HttpGet("preview-checkout")]
    public async Task<IActionResult> CheckoutPreview(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _checkoutService.PreviewCheckoutAsync(userId.Value, cancellationToken);
        return FromResult(result);
    }

    // ── Private helper ───────────────────────────────────────────────────────

    private Guid? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}