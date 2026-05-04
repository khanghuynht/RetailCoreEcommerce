using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Contracts.Models.Cart;
using RetailCoreEcommerce.Contracts.Models.Order;
using RetailCoreEcommerce.Contracts.Shared;
using RetailCoreEcommerce.Contracts.Utils;
using RetailCoreEcommerce.Domain;
using RetailCoreEcommerce.Domain.Constants;

namespace RetailCoreEcommerce.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDataCache _dataCache;
    private readonly IStripeService _stripeService;

    public OrderService(
        IUnitOfWork unitOfWork,
        IDataCache dataCache,
        IStripeService stripeService)
    {
        _unitOfWork = unitOfWork;
        _dataCache = dataCache;
        _stripeService = stripeService;
    }

    public async Task<Result<PlaceOrderResponse>> PlaceOrderAsync(
        Guid userId, PlaceOrderRequest request, CancellationToken ct = default)
    {
        try
        {
            var cart = await _dataCache.GetCacheAsync<Cart>(Helper.GetCartKey(userId));

            if (cart is null || cart.Items.Count == 0)
                return Result.Failure<PlaceOrderResponse>(
                    new Error("OrderService.PlaceOrderAsync", "Your cart is empty."));

            var productRepo = _unitOfWork.GetRepository<Product, Guid>();
            var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();

            var products = productRepo.FindAll(
                    p => productIds.Contains(p.Id),
                    tracking: false,
                    cancellationToken: ct,
                    p => p.Inventory)
                .ToDictionary(x => x.Id);

            decimal total = 0;

            foreach (var item in cart.Items)
            {
                if (!products.TryGetValue(item.ProductId, out var product) || !product.IsActive)

                    return Result.Failure<PlaceOrderResponse>(new Error(
                        "OrderService.PlaceOrderAsync",
                        $"Product '{item.ProductTitle}' is no longer available."));

                var available = product.Inventory.StockQuantity;

                if (item.Quantity > available)
                    return Result.Failure<PlaceOrderResponse>(new Error(
                        "OrderService.PlaceOrderAsync",
                        $"Not enough stock for '{item.ProductTitle}'. Available: {available}."));

                var unitPrice = product.SalePrice ?? product.OriginalPrice;
                total += unitPrice * item.Quantity;
            }

            var orderCode = GenerateOrderCode();

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OrderCode = orderCode,
                TotalAmount = total,
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                RecipientName = request.RecipientName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                StreetAddress = request.StreetAddress,
                Province = request.Province,
                Ward = request.Ward,
                Notes = request.Notes ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var orderItems = cart.Items.Select(item =>
            {
                var product = products[item.ProductId];
                var unitPrice = product.SalePrice ?? product.OriginalPrice;

                return new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    ProductTitle = item.ProductTitle,
                    ProductName = item.ProductTitle,
                    SKU = product.SKU,
                    ThumbnailUrl = item.ThumbnailUrl,
                    UnitPrice = unitPrice,
                    Quantity = item.Quantity,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            }).ToList();

            var history = new OrderHistory
            {
                OrderId = order.Id,
                OldStatus = string.Empty,
                NewStatus = nameof(OrderStatus.Pending)
            };

            var orderRepo = _unitOfWork.GetRepository<Order, Guid>();
            var orderItemRepo = _unitOfWork.GetRepository<OrderItem, Guid>();
            var historyRepo = _unitOfWork.GetRepository<OrderHistory, Guid>();

            orderRepo.Add(order);
            orderItemRepo.AddRange(orderItems);
            historyRepo.Add(history);

            await _unitOfWork.SaveChangesAsync();

            // Create Stripe PaymentIntent
            var (paymentIntentId, clientSecret) = await _stripeService.CreatePaymentIntentAsync(
                total, "usd", order.Id, ct);

            // Attach StripePaymentIntentId to the order
            order.StripePaymentIntentId = paymentIntentId;

            orderRepo.Update(order);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success(new PlaceOrderResponse
            {
                OrderId = order.Id,
                OrderCode = order.OrderCode,
                ClientSecret = clientSecret,
                PublishableKey = _stripeService.PublishableKey,
                TotalAmount = total
            });
        }
        catch (Exception ex)
        {
            return Result.Failure<PlaceOrderResponse>(
                new Error("OrderService.PlaceOrderAsync", ex.Message));
        }
    }

    public async Task<Result> HandlePaymentSucceededAsync(string paymentIntentId, CancellationToken ct = default)
    {
        try
        {
            var orderRepo = _unitOfWork.GetRepository<Order, Guid>();

            var order = await orderRepo.FindFirstAsync(
                o => o.StripePaymentIntentId == paymentIntentId,
                tracking: true,
                cancellationToken: ct,
                o => o.OrderItems);

            if (order is null)
                return Result.Failure(new Error("OrderService.HandlePaymentSucceededAsync",
                    $"Order not found for PaymentIntent {paymentIntentId}."));

            var oldStatus = order.Status.ToString();

            order.PaymentStatus = PaymentStatus.Succeeded;
            order.Status = OrderStatus.Paid;
            order.UpdatedAt = DateTime.UtcNow;

            var historyRepo = _unitOfWork.GetRepository<OrderHistory, Guid>();
            historyRepo.Add(new OrderHistory
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                OldStatus = oldStatus,
                NewStatus = nameof(OrderStatus.Paid)
            });

            // Deduct inventory (single query + in-memory updates)
            var inventoryRepo = _unitOfWork.GetRepository<Inventory, Guid>();
            var productIds = order.OrderItems.Select(i => i.ProductId).Distinct().ToList();

            var inventoriesByProduct = inventoryRepo.FindAll(
                    i => productIds.Contains(i.ProductId),
                    tracking: true,
                    cancellationToken: ct)
                .ToDictionary(x => x.ProductId);

            var quantityToDeductByProduct = order.OrderItems
                .GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

            foreach (var (productId, qty) in quantityToDeductByProduct)
            {
                if (!inventoriesByProduct.TryGetValue(productId, out var inventory))
                    continue;

                inventory.StockQuantity = Math.Max(0, inventory.StockQuantity - qty);
                inventory.ReservedQuantity = Math.Max(0, inventory.ReservedQuantity + qty);

                inventory.UpdatedAt = DateTime.UtcNow;
                inventoryRepo.Update(inventory);
            }

            orderRepo.Update(order);
            await _unitOfWork.SaveChangesAsync();

            await _dataCache.DeleteCacheAsync(Helper.GetCartKey(order.UserId));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("OrderService.HandlePaymentSucceededAsync", ex.Message));
        }
    }

    public async Task<Result> HandlePaymentFailedAsync(string paymentIntentId, CancellationToken ct = default)
    {
        try
        {
            var orderRepo = _unitOfWork.GetRepository<Order, Guid>();
            var order = await orderRepo.FindFirstAsync(
                o => o.StripePaymentIntentId == paymentIntentId,
                tracking: true,
                cancellationToken: ct);

            if (order is null)
                return Result.Failure(new Error("OrderService.HandlePaymentFailedAsync",
                    $"Order not found for PaymentIntent {paymentIntentId}."));

            var oldStatus = order.Status.ToString();

            order.PaymentStatus = PaymentStatus.Failed;
            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;

            var historyRepo = _unitOfWork.GetRepository<OrderHistory, Guid>();
            historyRepo.Add(new OrderHistory
            {
                OrderId = order.Id,
                OldStatus = oldStatus,
                NewStatus = nameof(OrderStatus.Cancelled)
            });

            orderRepo.Update(order);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("OrderService.HandlePaymentFailedAsync", ex.Message));
        }
    }

    private static string GenerateOrderCode()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString("N")[..8].ToUpper();
        return $"RC-{datePart}-{randomPart}";
    }
}