namespace RetailCoreEcommerce.Contracts.Models.User;

public class GetPagedUserResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Province { get; set; }
    public string? Ward { get; set; }
    public DateTime RegisteredAt { get; set; }   // maps to CreatedAt
}