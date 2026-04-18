using System.ComponentModel.DataAnnotations;
using RetailCoreEcommerce.Domain.Abstractions;

namespace RetailCoreEcommerce.Domain;

public class ProductRating : AuditableEntity<Guid>
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public int Rating { get; set; } // 1 to 5
    public string? Review { get; set; }
    public virtual Product Product { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}