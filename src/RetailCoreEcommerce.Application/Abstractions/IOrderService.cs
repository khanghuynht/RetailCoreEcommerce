using RetailCoreEcommerce.Contracts.Models.Order;
using RetailCoreEcommerce.Contracts.Shared;

namespace RetailCoreEcommerce.Application.Abstractions;

public interface IOrderService
{
    Task<Result<PlaceOrderResponse>> PlaceOrderAsync(Guid userId, PlaceOrderRequest request, CancellationToken ct = default);
    Task<Result> HandlePaymentSucceededAsync(string paymentIntentId, CancellationToken ct = default);
    Task<Result> HandlePaymentFailedAsync(string paymentIntentId, CancellationToken ct = default);
}
