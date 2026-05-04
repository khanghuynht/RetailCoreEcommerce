using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Application.Services;
using RetailCoreEcommerce.Infrastructure.Extensions;
using RetailCoreEcommerce.Persistence.Extensions;

namespace RetailCoreEcommerce.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureDatabaseConnection(configuration);
    }

    public static void AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductImageService, ProductImageService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<ICheckoutService, CheckoutService>();
        services.AddScoped<IOrderService, OrderService>();
    }

    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureCloudinary(configuration);
        services.ConfigureJwtSecurityToken();
        services.ConfigureBCryptPasswordHasher();
        services.ConfigureRedisCache(configuration);
        services.ConfigureStripe(configuration);
    }
}