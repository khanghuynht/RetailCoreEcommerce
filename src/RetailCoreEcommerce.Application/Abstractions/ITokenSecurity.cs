using RetailCoreEcommerce.Contracts.Models.Token;
using RetailCoreEcommerce.Domain;

namespace RetailCoreEcommerce.Application.Abstractions;

public interface ITokenSecurity
{
    string GenerateAccessToken(User user);
    TokenValidationResult ValidateAccessToken(string token, bool validateLifetime = true);
    DateTime? GetExpirationTimeFromToken(string token);
    
    string GenerateRefreshToken();
    TimeSpan GetRefreshTokenExpiry();
}