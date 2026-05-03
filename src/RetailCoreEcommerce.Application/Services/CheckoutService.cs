using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Contracts.Models.Cart;
using RetailCoreEcommerce.Contracts.Models.Checkout;
using RetailCoreEcommerce.Contracts.Shared;
using RetailCoreEcommerce.Domain;

namespace RetailCoreEcommerce.Application.Services;

public class CheckoutService : ICheckoutService
{
    private readonly IDataCache _dataCache;
    private readonly IUnitOfWork _unitOfWork;

    public CheckoutService(IDataCache dataCache, IUnitOfWork unitOfWork)
    {
        _dataCache = dataCache;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Result<PreviewCheckoutResponse>> PreviewCheckoutAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            var cart = await _dataCache.GetCacheAsync<Cart>(CartKey(userId))
                       ?? new Cart { UserId = userId, UpdatedAt = DateTime.UtcNow };

            if (cart.Items.Count == 0)
                return Result.Success(new PreviewCheckoutResponse { UserId = userId });

            var productRepo = _unitOfWork.GetRepository<Product, Guid>();
            var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();
            
            var products = productRepo.FindAll(
                p => productIds.Contains(p.Id),
                tracking: false,
                cancellationToken: ct,
                p => p.Inventory).ToDictionary(x => x.Id);

            var previewItems = cart.Items.Select(item =>
            {
                if (!products.TryGetValue(item.ProductId, out var product) || !product.IsActive)
                {
                    return new PreviewCheckoutItemResponse
                    {
                        ProductId = item.ProductId,
                        ProductTitle = item.ProductTitle,
                        ThumbnailUrl = item.ThumbnailUrl,
                        CachedUnitPrice = item.UnitPrice,
                        CurrentUnitPrice = item.UnitPrice,
                        Quantity = item.Quantity,
                        IsAvailable = false,
                        AvailableStock = 0
                    };
                }

                var currentPrice = product.SalePrice ?? product.OriginalPrice;
                var availableStock = product.Inventory.StockQuantity;

                return new PreviewCheckoutItemResponse
                {
                    ProductId = product.Id,
                    ProductTitle = product.Title,
                    ThumbnailUrl = product.ThumbnailUrl,
                    CachedUnitPrice = item.UnitPrice,
                    CurrentUnitPrice = currentPrice,
                    Quantity = item.Quantity,
                    IsAvailable = true,
                    AvailableStock = availableStock
                };
            }).ToList();

            return Result.Success(new PreviewCheckoutResponse
            {
                UserId = userId,
                Items = previewItems
            });
        }
        catch (Exception ex)
        {
            return Result.Failure<PreviewCheckoutResponse>(
                new Error("CartService.PreviewCheckoutAsync", ex.Message));
        }
    }
    
    private static string CartKey(Guid userId) => $"cart:{userId}";
}