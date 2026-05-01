namespace RetailCoreEcommerce.Contracts.Settings;

public class RedisSettings
{
    public static readonly string Section = "RedisSettings";
    public string EndPoints { get; set; } = null!;
    public int Port { get; set; }
    public string User { get; set; } = null!;
    public string Password { get; set; } = null!;
}