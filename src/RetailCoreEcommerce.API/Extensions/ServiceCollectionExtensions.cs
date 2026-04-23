using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Application.Services;

namespace RetailCoreEcommerce.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICategoryService, CategoryService>();
    }
}