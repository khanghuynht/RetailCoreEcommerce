namespace RetailCoreEcommerce.Contracts.Models.Auth;

public class LoginRequest
{
    public string LoginIdentifier { get; set; } = null!; // email or username
    public string Password { get; set; } = null!;
}