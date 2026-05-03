using CartModel = RetailCoreEcommerce.Contracts.Models.Cart.Cart;
using RetailCoreEcommerce.Contracts.Models.Cart;
using RetailCoreEcommerce.StoreFront.Models;

namespace RetailCoreEcommerce.StoreFront.Services.Cart;

public interface ICartApiService
{
    Task<ApiResponse<CartModel>> GetCartAsync(CancellationToken ct = default);
    Task<ApiResponse<CartModel>> AddItemAsync(AddCartItemRequest request, CancellationToken ct = default);
    Task<ApiResponse<CartModel>> UpdateItemAsync(UpdateCartItemRequest request, CancellationToken ct = default);
    Task<ApiResponse<CartModel>> RemoveItemAsync(Guid productId, CancellationToken ct = default);
    Task<ApiResponse<object>> ClearCartAsync(CancellationToken ct = default);
}
