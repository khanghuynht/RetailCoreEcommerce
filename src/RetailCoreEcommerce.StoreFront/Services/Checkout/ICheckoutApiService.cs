using RetailCoreEcommerce.Contracts.Models.Checkout;
using RetailCoreEcommerce.StoreFront.Models;

namespace RetailCoreEcommerce.StoreFront.Services.Checkout;

public interface ICheckoutApiService
{
    Task<ApiResponse<PreviewCheckoutResponse>> GetPreviewAsync(CancellationToken ct = default);
}
