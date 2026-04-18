using RetailCoreEcommerce.Domain.Abstractions;

namespace RetailCoreEcommerce.Domain;

public class Inventory : AuditableEntity<Guid>
{
    public Guid ProductId { get; set; }
    public int StockQuantity { get; set; }
    public int ReservedQuantity { get; set; } // Quantity reserved for pending orders
    public int SoldQuantity { get; set; } // Quantity sold
    public virtual Product Product { get; set; } = null!;
}