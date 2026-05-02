using System.Text.Json;
using RetailCoreEcommerce.Contracts.Models.Product;
using RetailCoreEcommerce.Contracts.Shared;
using RetailCoreEcommerce.StoreFront.Models;

namespace RetailCoreEcommerce.StoreFront.Services.Product;

public class ProductApiService : IProductApiService
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ProductApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<PaginationResult<GetAllProductResponse>>> GetPagedAsync(
        GetProductRequest request,
        CancellationToken ct = default)
    {
        var query = BuildQueryString(request);
        var response = await _httpClient.GetAsync($"v1/products{query}", ct);
        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<ApiResponse<PaginationResult<GetAllProductResponse>>>(json, JsonOptions)
               ?? new ApiResponse<PaginationResult<GetAllProductResponse>>();
    }

    public async Task<ApiResponse<GetProductResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"v1/products/{id}", ct);
        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<ApiResponse<GetProductResponse>>(json, JsonOptions)
               ?? new ApiResponse<GetProductResponse>();
    }

    private static string BuildQueryString(GetProductRequest request)
    {
        var parts = new List<string>
        {
            $"PageNumber={request.PageNumber}",
            $"PageSize={request.PageSize}"
        };

        if (!string.IsNullOrWhiteSpace(request.Name))
            parts.Add($"Name={Uri.EscapeDataString(request.Name)}");

        if (request.CategoryId.HasValue)
            parts.Add($"CategoryId={request.CategoryId.Value}");

        if (request.IsActive.HasValue)
            parts.Add($"IsActive={request.IsActive.Value}");
        

        return "?" + string.Join("&", parts);
    }
}
