using RetailCoreEcommerce.Contracts.Models.Checkout;
using RetailCoreEcommerce.Contracts.Shared;

namespace RetailCoreEcommerce.Application.Abstractions;

public interface ICheckoutService
{
    Task<Result<CheckoutPreviewResponse>> PreviewCheckoutAsync(Guid userId, CancellationToken ct = default);
}