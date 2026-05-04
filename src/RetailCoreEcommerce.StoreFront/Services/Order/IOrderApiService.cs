using RetailCoreEcommerce.Contracts.Models.Order;
using RetailCoreEcommerce.StoreFront.Models;

namespace RetailCoreEcommerce.StoreFront.Services.Order;

public interface IOrderApiService
{
    Task<ApiResponse<PlaceOrderResponse>> PlaceOrderAsync(PlaceOrderRequest request, CancellationToken ct = default);
}
