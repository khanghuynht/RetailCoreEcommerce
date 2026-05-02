using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Contracts.Models.Cart;
using RetailCoreEcommerce.Domain.Constants;

namespace RetailCoreEcommerce.API.Controllers;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/cart")]
[Authorize]
public class CartController : BaseApiController
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _cartService.GetCartAsync(userId.Value, cancellationToken);
        return FromResult(result);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(
        [FromBody] AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _cartService.AddItemAsync(userId.Value, request, cancellationToken);
        return FromResult(result);
    }

    [HttpPatch("items")]
    public async Task<IActionResult> UpdateItem(
        [FromBody] UpdateCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        
        var result = await _cartService.UpdateItemAsync(userId.Value, request, cancellationToken);
        return FromResult(result);
    }

    [HttpDelete("items")]
    public async Task<IActionResult> RemoveItem(
        [FromBody] RemoveCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _cartService.RemoveItemAsync(userId.Value, request, cancellationToken);
        return FromResult(result);
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _cartService.ClearCartAsync(userId.Value, cancellationToken);
        return FromResult(result);
    }
    

    // ── Private helper ───────────────────────────────────────────────────────

    private Guid? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
