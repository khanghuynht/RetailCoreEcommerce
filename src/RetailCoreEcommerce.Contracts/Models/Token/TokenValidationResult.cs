namespace RetailCoreEcommerce.Contracts.Models.Token;

public record TokenValidationResult(
    bool IsValid,
    string? UserId,
    IReadOnlyList<TokenClaim> Claims
);