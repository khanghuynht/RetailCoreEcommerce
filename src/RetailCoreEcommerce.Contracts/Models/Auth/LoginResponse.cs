namespace RetailCoreEcommerce.Contracts.Models.Auth;

public class LoginResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public DateTime? RegisteredAt { get; set; }
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}