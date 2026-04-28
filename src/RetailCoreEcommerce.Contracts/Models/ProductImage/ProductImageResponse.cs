namespace RetailCoreEcommerce.Contracts.Models.ProductImage;

public sealed class ProductImageResponse
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string? Name { get; set; }
    public int Position { get; set; }
}