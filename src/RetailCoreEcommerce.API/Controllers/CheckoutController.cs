using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Contracts.Models.Order;

namespace RetailCoreEcommerce.API.Controllers;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}")]
[Authorize]
public class CheckoutController : BaseApiController
{
    private readonly ICheckoutService _checkoutService;
    private readonly IOrderService _orderService;

    public CheckoutController(ICheckoutService checkoutService, IOrderService orderService)
    {
        _checkoutService = checkoutService;
        _orderService = orderService;
    }

    [HttpGet("preview-checkout")]
    public async Task<IActionResult> CheckoutPreview(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _checkoutService.PreviewCheckoutAsync(userId.Value, cancellationToken);
        return FromResult(result);
    }

    [HttpPost("place-order")]
    public async Task<IActionResult> PlaceOrder(
        [FromBody] PlaceOrderRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _orderService.PlaceOrderAsync(userId.Value, request, cancellationToken);
        return FromResult(result);
    }

    // ── Private helper ───────────────────────────────────────────────────────

    private Guid? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}