using RetailCoreEcommerce.StoreFront.Services;
using RetailCoreEcommerce.StoreFront.Services.Auth;
using RetailCoreEcommerce.StoreFront.Services.Category;
using RetailCoreEcommerce.StoreFront.Services.Product;
using RetailCoreEcommerce.StoreFront.Services.TokenStorage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");

// Required for IHttpContextAccessor (used by CookieTokenStorageService and AuthTokenHandler)
builder.Services.AddHttpContextAccessor();

// Token storage — reads/writes HTTP-only cookies
builder.Services.AddScoped<ITokenStorageService, CookieTokenStorageService>();

// DelegatingHandler that attaches the Bearer token to every outbound API call
builder.Services.AddTransient<AuthTokenHandler>();

// Product and Category clients — Bearer token is attached automatically via AuthTokenHandler
builder.Services.AddHttpClient<IProductApiService, ProductApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<ICategoryApiService, CategoryApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthTokenHandler>();

// Auth client — does NOT use AuthTokenHandler; auth endpoints are public
builder.Services.AddHttpClient<IAuthApiService, AuthApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
