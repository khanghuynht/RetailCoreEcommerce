using RetailCoreEcommerce.Domain.Abstractions;

namespace RetailCoreEcommerce.Domain;

public class OrderHistory : AuditableEntity<Guid>
{
    public Guid OrderId { get; set; }
    public string OldStatus { get; set; } = null!;
    public string NewStatus { get; set; } = null!;
    public virtual Order Order { get; set; } = null!;
}