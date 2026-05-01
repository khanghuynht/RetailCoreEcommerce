namespace RetailCoreEcommerce.Contracts.Models.Auth;

public class RefreshTokenRequest
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}