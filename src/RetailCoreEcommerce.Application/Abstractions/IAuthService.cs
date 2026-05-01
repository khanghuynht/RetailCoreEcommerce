using RetailCoreEcommerce.Contracts.Models.Auth;
using RetailCoreEcommerce.Contracts.Shared;

namespace RetailCoreEcommerce.Application.Abstractions;

public interface IAuthService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<Result<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
    Task<Result> LogoutAsync(RefreshTokenRequest request, CancellationToken ct);
}