using System.Net.Http.Json;
using System.Text.Json;
using CartModel = RetailCoreEcommerce.Contracts.Models.Cart.Cart;
using RetailCoreEcommerce.Contracts.Models.Cart;
using RetailCoreEcommerce.StoreFront.Models;

namespace RetailCoreEcommerce.StoreFront.Services.Cart;

public class CartApiService(HttpClient httpClient) : ICartApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<ApiResponse<CartModel>> GetCartAsync(CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync("v1/cart", ct);
        return await DeserializeAsync<CartModel>(response, ct);
    }

    public async Task<ApiResponse<CartModel>> AddItemAsync(AddCartItemRequest request, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync("v1/cart/items", request, ct);
        return await DeserializeAsync<CartModel>(response, ct);
    }

    public async Task<ApiResponse<CartModel>> UpdateItemAsync(UpdateCartItemRequest request, CancellationToken ct = default)
    {
        var response = await httpClient.PatchAsJsonAsync("v1/cart/items", request, ct);
        return await DeserializeAsync<CartModel>(response, ct);
    }

    public async Task<ApiResponse<CartModel>> RemoveItemAsync(Guid productId, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, "v1/cart/items")
        {
            Content = JsonContent.Create(new RemoveCartItemRequest { ProductId = productId })
        };
        var response = await httpClient.SendAsync(request, ct);
        return await DeserializeAsync<CartModel>(response, ct);
    }

    public async Task<ApiResponse<object>> ClearCartAsync(CancellationToken ct = default)
    {
        var response = await httpClient.DeleteAsync("v1/cart", ct);
        return await DeserializeAsync<object>(response, ct);
    }

    private static async Task<ApiResponse<T>> DeserializeAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        var json = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(json))
            return new ApiResponse<T> { IsSuccess = response.IsSuccessStatusCode };
        return JsonSerializer.Deserialize<ApiResponse<T>>(json, JsonOptions) ?? new ApiResponse<T>();
    }
}
