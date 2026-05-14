using RetailCoreEcommerce.Contracts.Models.Token;

namespace RetailCoreEcommerce.Contracts.Infrastructure;

public interface ITokenSecurity
{
    string GenerateAccessToken(UserClaim user);
    TokenValidationResult ValidateAccessToken(string token, bool validateLifetime = true);
    DateTime? GetExpirationTimeFromToken(string token);
    
    string GenerateRefreshToken();
    TimeSpan GetRefreshTokenExpiry();
}