namespace RetailCoreEcommerce.Contracts.Abstractions.Security;

public sealed record UserClaim(
    string Id,
    string Email,
    string Username,
    string FirstName,
    string LastName,
    string Role);