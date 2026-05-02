using RetailCoreEcommerce.Contracts.Models.Auth;
using RetailCoreEcommerce.StoreFront.Models;

namespace RetailCoreEcommerce.StoreFront.Services.Auth;

public interface IAuthApiService
{
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<ApiResponse<object>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<ApiResponse<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
    Task<ApiResponse<object>> LogoutAsync(RefreshTokenRequest request, CancellationToken ct = default);
}
