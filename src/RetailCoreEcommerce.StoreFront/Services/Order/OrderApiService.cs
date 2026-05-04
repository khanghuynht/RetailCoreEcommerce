using System.Text.Json;
using RetailCoreEcommerce.Contracts.Models.Order;
using RetailCoreEcommerce.StoreFront.Models;

namespace RetailCoreEcommerce.StoreFront.Services.Order;

public class OrderApiService(HttpClient httpClient) : IOrderApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<ApiResponse<PlaceOrderResponse>> PlaceOrderAsync(
        PlaceOrderRequest request, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync("v1/place-order", request, ct);
        return await DeserializeAsync<PlaceOrderResponse>(response, ct);
    }

    private static async Task<ApiResponse<T>> DeserializeAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        var json = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(json))
            return new ApiResponse<T> { IsSuccess = response.IsSuccessStatusCode };
        return JsonSerializer.Deserialize<ApiResponse<T>>(json, JsonOptions) ?? new ApiResponse<T>();
    }
}
