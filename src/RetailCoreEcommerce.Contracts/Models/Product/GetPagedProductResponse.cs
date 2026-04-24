namespace RetailCoreEcommerce.Contracts.Models.Product;

public class GetAllProductResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string SKU { get; set; } = null!;
    public decimal OriginalPrice { get; set; }
    public decimal? SalePrice { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string Status { get; set; } = null!;
    public bool IsActive { get; set; }
    public Guid CategoryId { get; set; }
}