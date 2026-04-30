using RetailCoreEcommerce.Contracts.Shared;

namespace RetailCoreEcommerce.Contracts.Models.Product;

public class GetProductRequest : PaginationParams
{
    public string? Name { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? IsActive { get; set; }
}