using RetailCoreEcommerce.Persistence.Extensions;

namespace RetailCoreEcommerce.API;

public static class Startup
{
    public static void Configure(this WebApplicationBuilder builder)
    {
        builder.Services.AddPersistence(builder.Configuration);
    }
}