using RetailCoreEcommerce.Domain.Abstractions;

namespace RetailCoreEcommerce.Domain;

public class OrderItem : AuditableEntity<Guid>
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string ProductName { get; set; } = null!;
    public string ProductTitle { get; set; } = null!;
    public string SKU { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public virtual Order Order { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}