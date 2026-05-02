using RetailCoreEcommerce.Contracts.Models.Cart;
using RetailCoreEcommerce.Contracts.Models.Checkout;
using RetailCoreEcommerce.Contracts.Shared;

namespace RetailCoreEcommerce.Application.Abstractions;

public interface ICartService
{
    Task<Result<Cart>> GetCartAsync(Guid userId, CancellationToken ct = default);
    Task<Result<Cart>> AddItemAsync(Guid userId, AddCartItemRequest request, CancellationToken ct = default);
    Task<Result<Cart>> UpdateItemAsync(Guid userId, UpdateCartItemRequest request, CancellationToken ct = default);
    Task<Result> RemoveItemAsync(Guid userId, RemoveCartItemRequest request, CancellationToken ct = default);
    Task<Result> ClearCartAsync(Guid userId, CancellationToken ct = default);
}