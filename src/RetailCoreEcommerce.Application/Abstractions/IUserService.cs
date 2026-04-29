using RetailCoreEcommerce.Contracts.Models.User;
using RetailCoreEcommerce.Contracts.Shared;

namespace RetailCoreEcommerce.Application.Abstractions;

public interface IUserService
{
    Task<Result<PaginationResult<GetPagedUserResponse>>> GetPagedCustomersAsync(
        GetPagedUserRequest request,
        CancellationToken cancellationToken = default);
}