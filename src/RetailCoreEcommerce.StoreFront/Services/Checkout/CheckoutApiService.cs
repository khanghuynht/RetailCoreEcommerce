using System.Text.Json;
using RetailCoreEcommerce.Contracts.Models.Checkout;
using RetailCoreEcommerce.StoreFront.Models;

namespace RetailCoreEcommerce.StoreFront.Services.Checkout;

public class CheckoutApiService(HttpClient httpClient) : ICheckoutApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<ApiResponse<PreviewCheckoutResponse>> GetPreviewAsync(CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync("v1/preview-checkout", ct);
        return await DeserializeAsync<PreviewCheckoutResponse>(response, ct);
    }

    private static async Task<ApiResponse<T>> DeserializeAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        var json = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(json))
            return new ApiResponse<T> { IsSuccess = response.IsSuccessStatusCode };
        return JsonSerializer.Deserialize<ApiResponse<T>>(json, JsonOptions) ?? new ApiResponse<T>();
    }
}
