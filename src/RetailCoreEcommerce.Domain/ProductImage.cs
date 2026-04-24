using System.ComponentModel.DataAnnotations;
using RetailCoreEcommerce.Domain.Abstractions;

namespace RetailCoreEcommerce.Domain;

public class ProductImage : AuditableEntity<Guid>
{
    public Guid ProductId { get; set; }
    public string? Name { get; set; }
    public string ImageUrl { get; set; } = null!;

    public string PublicId { get; set; } = null!;  // For cloud storage reference
    
    public int Position { get; set; } // For ordering images
    public virtual Product Product { get; set; } = null!;
}