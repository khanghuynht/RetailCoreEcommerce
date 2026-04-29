using RetailCoreEcommerce.Contracts.Models.Category;
using RetailCoreEcommerce.Contracts.Models.Product;
using RetailCoreEcommerce.Contracts.Shared;

namespace RetailCoreEcommerce.StoreFront.Models;

public class ShopViewModel
{
    public PaginationResult<GetAllProductResponse>? Products { get; set; }
    public List<GetPagedCategoryResponse> Categories { get; set; } = [];

    // Filter state — preserved across requests to keep UI in sync
    public string? SearchName { get; set; }
    public Guid? SelectedCategoryId { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
