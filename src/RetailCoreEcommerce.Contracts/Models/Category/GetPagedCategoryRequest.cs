using RetailCoreEcommerce.Contracts.Shared;

namespace RetailCoreEcommerce.Contracts.Models.Category;

public class GetAllCategoriesRequest : PaginationParams
{
    public string? Name { get; set; }
    public Guid? ParentId { get; set; }
    public bool? IsRootOnly { get; set; }   // filter top-level only
}