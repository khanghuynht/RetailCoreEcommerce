using System.Text.Json;
using RetailCoreEcommerce.Contracts.Models.Category;
using RetailCoreEcommerce.Contracts.Shared;
using RetailCoreEcommerce.StoreFront.Models;

namespace RetailCoreEcommerce.StoreFront.Services.Category;

public class CategoryApiService : ICategoryApiService
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public CategoryApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<PaginationResult<GetPagedCategoryResponse>>> GetAllAsync(
        GetAllCategoriesRequest request,
        CancellationToken ct = default)
    {
        var query = BuildQueryString(request);
        var response = await _httpClient.GetAsync($"v1/categories{query}", ct);
        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<ApiResponse<PaginationResult<GetPagedCategoryResponse>>>(json, JsonOptions)
               ?? new ApiResponse<PaginationResult<GetPagedCategoryResponse>>();
    }

    private static string BuildQueryString(GetAllCategoriesRequest request)
    {
        var parts = new List<string>
        {
            $"PageNumber={request.PageNumber}",
            $"PageSize={request.PageSize}"
        };

        if (!string.IsNullOrWhiteSpace(request.Name))
            parts.Add($"Name={Uri.EscapeDataString(request.Name)}");

        if (request.ParentId.HasValue)
            parts.Add($"ParentId={request.ParentId.Value}");

        if (request.IsRootOnly.HasValue)
            parts.Add($"IsRootOnly={request.IsRootOnly.Value}");

        return "?" + string.Join("&", parts);
    }
}
