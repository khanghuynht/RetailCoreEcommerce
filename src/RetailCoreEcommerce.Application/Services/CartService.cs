using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Contracts.Models.Cart;
using RetailCoreEcommerce.Contracts.Models.Checkout;
using RetailCoreEcommerce.Contracts.Shared;
using RetailCoreEcommerce.Contracts.Utils;
using RetailCoreEcommerce.Domain;

namespace RetailCoreEcommerce.Application.Services;

public class CartService : ICartService
{
    private readonly IDataCache _dataCache;
    private readonly IUnitOfWork _unitOfWork;

    // Temporary hardcoded (Can move to config later)
    // Cart data will be stored in cache for 14 days to balance between user convenience and data freshness.
    private readonly TimeSpan _cartTtl = TimeSpan.FromDays(14);

    public CartService(IDataCache dataCache, IUnitOfWork unitOfWork)
    {
        _dataCache = dataCache;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Cart>> GetCartAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            var cart = await _dataCache.GetCacheAsync<Cart>(Helper.GetCartKey(userId))
                       ?? new Cart { UserId = userId, UpdatedAt = DateTime.UtcNow };

            return Result.Success(cart);
        }
        catch (Exception ex)
        {
            return Result.Failure<Cart>(new Error("CartService.GetCartAsync", ex.Message));
        }
    }

    public async Task<Result<Cart>> AddItemAsync(Guid userId, AddCartItemRequest request,
        CancellationToken ct = default)
    {
        try
        {
            if (request.Quantity <= 0)
                return Result.Failure<Cart>(new Error("CartService.AddItemAsync", "Quantity must be greater than 0."));

            var productRepo = _unitOfWork.GetRepository<Product, Guid>();
            var product = await productRepo.FindByIdAsync(
                request.ProductId,
                tracking: false,
                cancellationToken: ct,
                p => p.Inventory);

            if (product is null || !product.IsActive)
                return Result.Failure<Cart>(new Error("CartService.AddItemAsync", "Product not found or unavailable."));

            var availableStock = product.Inventory.StockQuantity;

            var cart = await _dataCache.GetCacheAsync<Cart>(Helper.GetCartKey(userId))
                       ?? new Cart { UserId = userId, UpdatedAt = DateTime.UtcNow };

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);

            if (existingItem is not null)
            {
                var newQuantity = existingItem.Quantity + request.Quantity;

                if (newQuantity > availableStock)
                    return Result.Failure<Cart>(new Error("CartService.AddItemAsync",
                        $"Not enough stock. Available: {availableStock}, In cart: {existingItem.Quantity}."));

                existingItem.Quantity = newQuantity;
            }
            else
            {
                if (request.Quantity > availableStock)
                    return Result.Failure<Cart>(new Error("CartService.AddItemAsync",
                        $"Not enough stock. Available: {availableStock}."));

                cart.Items.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductTitle = product.Title,
                    ThumbnailUrl = product.ThumbnailUrl,
                    UnitPrice = product.SalePrice ?? product.OriginalPrice,
                    Quantity = request.Quantity
                });
            }

            cart.UpdatedAt = DateTime.UtcNow;

            await _dataCache.SetCacheAsync(Helper.GetCartKey(userId), cart, _cartTtl);

            return Result.Success(cart);
        }
        catch (Exception ex)
        {
            return Result.Failure<Cart>(new Error("CartService.AddItemAsync", ex.Message));
        }
    }

    public async Task<Result<Cart>> UpdateItemAsync(Guid userId, UpdateCartItemRequest request,
        CancellationToken ct = default)
    {
        try
        {
            if (request.Quantity < 0)
                return Result.Failure<Cart>(new Error("CartService.UpdateItemAsync", "Quantity cannot be negative."));

            var cart = await _dataCache.GetCacheAsync<Cart>(Helper.GetCartKey(userId))
                       ?? new Cart { UserId = userId, UpdatedAt = DateTime.UtcNow };

            var item = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);

            if (item is null)
                return Result.Failure<Cart>(new Error("CartService.UpdateItemAsync", "Item not found in cart."));

            if (request.Quantity == 0)
            {
                cart.Items.Remove(item);
            }
            else
            {
                var productRepo = _unitOfWork.GetRepository<Product, Guid>();

                var product = await productRepo.FindByIdAsync(
                    request.ProductId,
                    tracking: false,
                    cancellationToken: ct,
                    p => p.Inventory);

                if (product is null || !product.IsActive)
                    return Result.Failure<Cart>(new Error("CartService.UpdateItemAsync",
                        "Product not found or unavailable."));

                var availableStock = product.Inventory.StockQuantity - product.Inventory.ReservedQuantity;

                if (request.Quantity > availableStock)
                    return Result.Failure<Cart>(new Error("CartService.UpdateItemAsync",
                        $"Not enough stock. Available: {availableStock}."));

                item.Quantity = request.Quantity;
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _dataCache.SetCacheAsync(Helper.GetCartKey(userId), cart, _cartTtl);

            return Result.Success(cart);
        }
        catch (Exception ex)
        {
            return Result.Failure<Cart>(new Error("CartService.UpdateItemAsync", ex.Message));
        }
    }

    public async Task<Result> RemoveItemAsync(Guid userId, RemoveCartItemRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var cart = await _dataCache.GetCacheAsync<Cart>(Helper.GetCartKey(userId))
                       ?? new Cart { UserId = userId, UpdatedAt = DateTime.UtcNow };

            var item = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);

            if (item is null)
                return Result.Failure(new Error("CartService.RemoveItemAsync", "Item not found in cart."));

            cart.Items.Remove(item);
            cart.UpdatedAt = DateTime.UtcNow;

            await _dataCache.SetCacheAsync(Helper.GetCartKey(userId), cart, _cartTtl);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("CartService.RemoveItemAsync", ex.Message));
        }
    }

    public async Task<Result> ClearCartAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            await _dataCache.DeleteCacheAsync(Helper.GetCartKey(userId));
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("CartService.ClearCartAsync", ex.Message));
        }
    }
    
}