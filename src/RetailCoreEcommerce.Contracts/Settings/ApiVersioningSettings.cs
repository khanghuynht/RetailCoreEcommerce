namespace RetailCoreEcommerce.Contracts.Settings;

public class ApiVersioningSettings
{
    public const string Section = "ApiVersioning";
    public int DefaultMajor { get; set; } = 1;
    public int DefaultMinor { get; set; } = 0;
    public bool AssumeDefaultWhenUnspecified { get; set; } = true;
    public bool ReportApiVersions { get; set; } = true;
    public string Reader { get; set; } = "UrlSegment";
}