using RetailCoreEcommerce.Domain.Abstractions;

namespace RetailCoreEcommerce.Domain;

public class Category : AuditableEntity<Guid>
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; } // For hierarchical categories, 0 or null for top-level
    
    public virtual Category? Parent { get; set; } // Navigation property for parent category
    public ICollection<Category> Children { get; set; } = []; // Navigation property for child categories
    public virtual ICollection<Product> Products { get; set; } = [];
}