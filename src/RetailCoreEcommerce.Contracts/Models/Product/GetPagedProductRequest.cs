using RetailCoreEcommerce.Contracts.Constants;
using RetailCoreEcommerce.Contracts.Shared;

namespace RetailCoreEcommerce.Contracts.Models.Product;

public class GetProductRequest : PaginationParams
{
    public string? Name { get; set; }
    public Guid? CategoryId { get; set; }
    public ProductStatus? Status { get; set; }
    public bool? IsActive { get; set; }
}