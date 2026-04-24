using System.Text.Json.Serialization;

namespace RetailCoreEcommerce.Contracts.Models.Product;

public class UpdateProductRequest
{
    [JsonIgnore] public Guid Id { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Title { get; set; }
    public string? Name { get; set; }
    public decimal? OriginalPrice { get; set; }
    public decimal? SalePrice { get; set; }
    public string? Description { get; set; }
    public int? Length { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public bool? IsActive { get; set; }
}