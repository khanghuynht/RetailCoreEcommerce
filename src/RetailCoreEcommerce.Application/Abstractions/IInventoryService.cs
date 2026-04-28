using RetailCoreEcommerce.Contracts.Models.Inventory;
using RetailCoreEcommerce.Contracts.Shared;

namespace RetailCoreEcommerce.Application.Abstractions;

public interface IInventoryService
{
    Task<Result> UpdateInventoryQuantityAsync(
        UpdateInventoryQuantityRequest request,
        CancellationToken cancellationToken = default);
}