using System.ComponentModel.DataAnnotations;
using RetailCoreEcommerce.Contracts.Domain;
using RetailCoreEcommerce.Domain.Constants;

namespace RetailCoreEcommerce.Domain;

public class User : AuditableEntity<Guid>
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Province { get; set; }
    public string? Ward { get; set; }
    public UserRole Role { get; set; }
    
    public virtual ICollection<Order>? Orders { get; set; } = [];
    public virtual ICollection<ProductRating>? ProductRatings { get; set; } = [];
}