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
        return await DeserializeAsync<PaginationResult<GetAllProductResponse>>(response, ct);
    }

    public async Task<ApiResponse<GetProductResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"v1/products/{id}", ct);
        return await DeserializeAsync<GetProductResponse>(response, ct);
    }

    private static async Task<ApiResponse<T>> DeserializeAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        var json = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(json))
            return new ApiResponse<T> { IsSuccess = response.IsSuccessStatusCode };
        return JsonSerializer.Deserialize<ApiResponse<T>>(json, JsonOptions) ?? new ApiResponse<T>();
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
