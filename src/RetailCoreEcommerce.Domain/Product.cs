using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RetailCoreEcommerce.Domain.Abstractions;
using RetailCoreEcommerce.Domain.Constants;

namespace RetailCoreEcommerce.Domain;

public class Product : AuditableEntity<Guid>
{
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = null!;
    public string SKU { get; set; } = null!; // Stock Keeping Unit, unique identifier for the product
    public string Name { get; set; } = null!;
    public decimal OriginalPrice { get; set; }
    public decimal? SalePrice { get; set; }
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int Length { get; set; } // in cm
    public int Width { get; set; } // in cm
    public int Height { get; set; } // in cm
    public ProductStatus Status { get; set; } // Draft, Published
    public bool IsActive { get; set; } // Visibility in storefront
    public virtual Category Category { get; set; } = null!;
    public virtual Inventory Inventory { get; set; } = null!;

    public virtual ICollection<ProductImage>? ProductImages { get; set; } = [];
    public virtual ICollection<ProductRating>? ProductRatings { get; set; } = [];
}