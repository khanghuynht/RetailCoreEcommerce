using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Contracts.Models.User;
using RetailCoreEcommerce.Contracts.Shared;
using RetailCoreEcommerce.Domain;
using RetailCoreEcommerce.Domain.Constants;

namespace RetailCoreEcommerce.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaginationResult<GetPagedUserResponse>>> GetPagedCustomersAsync(
        GetPagedUserRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userRepo = _unitOfWork.GetRepository<User, Guid>();

            var pagedResult = await userRepo.GetPagedAsync(
                predicate: x =>
                    x.Role == UserRole.Customer &&
                    (request.Name == null ||
                     x.FirstName.Contains(request.Name) ||
                     x.LastName.Contains(request.Name)) &&
                    (request.Email == null || x.Email.Contains(request.Email)),
                orderBy: q => q.OrderBy(x => x.CreatedAt),
                pagination: request,
                cancellationToken: cancellationToken);

            var responses = pagedResult.Items.Select(u => new GetPagedUserResponse
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                PhoneNumber = u.PhoneNumber,
                Address = u.Address,
                Province = u.Province,
                Ward = u.Ward,
                RegisteredAt = u.CreatedAt
            });

            return Result.Success(new PaginationResult<GetPagedUserResponse>(
                responses,
                pagedResult.TotalItems,
                pagedResult.PageNumber,
                pagedResult.PageSize));
        }
        catch (Exception ex)
        {
            return Result.Failure<PaginationResult<GetPagedUserResponse>>(
                new Error("UserService.GetPagedCustomersAsync",
                    $"Error retrieving customers: {ex.Message}"));
        }
    }
}