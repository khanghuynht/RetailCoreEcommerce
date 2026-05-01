using CloudinaryDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Contracts.Settings;
using RetailCoreEcommerce.Infrastructure.Bcrypt;
using RetailCoreEcommerce.Infrastructure.Cloudinary;
using RetailCoreEcommerce.Infrastructure.Jwt;
using RetailCoreEcommerce.Infrastructure.Redis;
using StackExchange.Redis;

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
        services.AddSingleton<ICloudinary>(sp =>
        {
            var s = sp.GetRequiredService<IOptions<CloudinarySettings>>().Value;
            return new CloudinaryDotNet.Cloudinary(new Account(s.CloudName, s.ApiKey, s.ApiSecret))
            {
                Api = { Secure = true }
            };
        });
        
        services.AddScoped<IImageStorage, CloudinaryImageStorage>();
    }
    
    public static void ConfigureJwtSecurityToken(this IServiceCollection services)
    {
        services.AddScoped<ITokenSecurity, JwtSecurityToken>();
    }
    
    public static void ConfigureBCryptPasswordHasher(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHashed, BCryptPasswordHashed>();
    }
    
    public static void ConfigureRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RedisSettings>()
            .Bind(configuration.GetSection(RedisSettings.Section))
            .Validate(s=> !string.IsNullOrWhiteSpace(s.EndPoints), "Redis:EndPoints is required")
            .Validate(s => !string.IsNullOrWhiteSpace(s.User), "Redis:User is required")
            .Validate(s => s.Port > 0, "Redis:Port must be greater than 0")
            .ValidateOnStart();
        
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisSettings = sp.GetRequiredService<IOptions<RedisSettings>>().Value;
            return ConnectionMultiplexer.Connect(
                $"{redisSettings.EndPoints}:{redisSettings.Port},user={redisSettings.User},password={redisSettings.Password}");
        });

        services.AddScoped<IDataCache, RedisService>();
    }
}