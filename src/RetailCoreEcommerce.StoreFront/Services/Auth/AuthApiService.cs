using System.Text.Json;
using RetailCoreEcommerce.Contracts.Models.Auth;
using RetailCoreEcommerce.StoreFront.Models;

namespace RetailCoreEcommerce.StoreFront.Services.Auth;

public class AuthApiService(HttpClient httpClient) : IAuthApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync("v1/auth/login", request, ct);
        return await DeserializeAsync<LoginResponse>(response, ct);
    }

    public async Task<ApiResponse<object>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync("v1/auth/register", request, ct);
        return await DeserializeAsync<object>(response, ct);
    }

    public async Task<ApiResponse<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync("v1/auth/refresh", request, ct);
        return await DeserializeAsync<LoginResponse>(response, ct);
    }

    public async Task<ApiResponse<object>> LogoutAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync("v1/auth/logout", request, ct);
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
