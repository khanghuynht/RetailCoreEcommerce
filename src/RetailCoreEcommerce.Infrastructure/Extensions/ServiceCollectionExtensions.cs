using CloudinaryDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Contracts.Settings;
using RetailCoreEcommerce.Infrastructure.Cloudinary;

namespace RetailCoreEcommerce.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureCloudinary(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<CloudinarySettings>()
            .Bind(configuration.GetSection(CloudinarySettings.Section))
            .Validate(s => !string.IsNullOrWhiteSpace(s.CloudName), "Cloudinary:CloudName is required")
            .Validate(s => !string.IsNullOrWhiteSpace(s.ApiKey), "Cloudinary:ApiKey is required")
            .Validate(s => !string.IsNullOrWhiteSpace(s.ApiSecret), "Cloudinary:ApiSecret is required")
            .ValidateOnStart();
        
        // Register one Cloudinary client for the whole app (Singleton)
        // This is the "object creation" part that belongs in composition root
        services.AddSingleton(sp =>
        {
            var s = sp.GetRequiredService<IOptions<CloudinarySettings>>().Value;
            return new CloudinaryDotNet.Cloudinary(new Account(s.CloudName, s.ApiKey, s.ApiSecret))
            {
                Api = { Secure = true }
            };
        });
        
        services.AddScoped<IImageStorage, CloudinaryImageStorage>();
    }
}