using RetailCoreEcommerce.Contracts.Models.Category;
using RetailCoreEcommerce.Contracts.Models.Product;

namespace RetailCoreEcommerce.StoreFront.Models;

public class HomeViewModel
{
    public List<GetPagedCategoryResponse> Categories { get; set; } = [];
    public List<GetAllProductResponse> FeaturedProducts { get; set; } = [];
}
