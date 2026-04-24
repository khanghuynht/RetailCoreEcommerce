namespace RetailCoreEcommerce.Contracts.Settings;

public sealed class CloudinarySettings
{
    public const string Section = "Cloudinary";
    public string CloudName { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
    public string ApiSecret { get; set; } = null!;
    
    // Optional folder in Cloudinary where images will be stored
    public string Folder { get; set; } = null!;
}