namespace RetailCoreEcommerce.Contracts.Shared;

public class JwtSettings
{
    public static readonly string Section = "JwtSettings";
    public string PrivateKeyBytes { get; set; } = null!;
    public string PublicKeyBytes { get; set; } = null!;
    public double AccessTokenExpirationMinutes { get; set; }
    public double RefreshTokenExpirationMinutes { get; set; }
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
}