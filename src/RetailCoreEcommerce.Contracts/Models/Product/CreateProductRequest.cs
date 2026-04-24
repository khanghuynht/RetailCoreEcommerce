using System.ComponentModel.DataAnnotations;

namespace RetailCoreEcommerce.Contracts.Models.Product;

public class CreateProductRequest
{
    public Guid CategoryId { get; set; }
    [Required] public string Title { get; set; } = null!;
    [Required] public string SKU { get; set; } = null!;
    [Required] public string Name { get; set; } = null!;
    public decimal OriginalPrice { get; set; }
    public decimal? SalePrice { get; set; }
    public string? Description { get; set; }
    public int Length { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int InitialStock { get; set; }
}