using RetailCoreEcommerce.Contracts.Shared;

namespace RetailCoreEcommerce.Contracts.Models.User;

public class GetPagedUserRequest : PaginationParams
{
    public string? Name { get; set; } // searches FirstName or LastName
    public string? Email { get; set; }
}