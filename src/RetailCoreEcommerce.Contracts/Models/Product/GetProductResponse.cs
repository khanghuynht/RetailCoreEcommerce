using RetailCoreEcommerce.Contracts.Models.ProductImage;

namespace RetailCoreEcommerce.Contracts.Models.Product;

public class GetProductResponse
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Title { get; set; } = null!;
    public string SKU { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal OriginalPrice { get; set; }
    public decimal? SalePrice { get; set; }
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int Length { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Status { get; set; } = null!;
    public bool IsActive { get; set; }
    public int StockQuantity { get; set; }

    // Gallery images ordered by position
    public List<ProductImageResponse> Images { get; set; } = [];
}