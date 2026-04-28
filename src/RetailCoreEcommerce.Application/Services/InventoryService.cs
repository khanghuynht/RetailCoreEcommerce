using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Contracts.Models.Inventory;
using RetailCoreEcommerce.Contracts.Shared;
using RetailCoreEcommerce.Domain;

namespace RetailCoreEcommerce.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public InventoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> UpdateInventoryQuantityAsync(
        UpdateInventoryQuantityRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var inventoryRepo = _unitOfWork.GetRepository<Inventory, Guid>();

            var inventory = await inventoryRepo.FindFirstAsync(
                x => x.ProductId == request.ProductId,
                tracking: true,
                cancellationToken: cancellationToken);

            if (inventory is null)
                return Result.Failure(new Error("Inventory.NotFound",
                    $"Inventory for product {request.ProductId} not found."));

            inventory.StockQuantity = request.StockQuantity;

            inventoryRepo.Update(inventory);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ProductService.UpdateInventoryQuantityAsync",
                $"Error updating inventory: {ex.Message}"));
        }
    }
    
}