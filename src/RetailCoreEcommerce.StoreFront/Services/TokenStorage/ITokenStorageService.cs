namespace RetailCoreEcommerce.StoreFront.Services.TokenStorage;

public interface ITokenStorageService
{
    /// <summary>Persists both tokens and the user's display name in secure HTTP-only cookies.</summary>
    void StoreSession(string accessToken, string refreshToken, string fullName);

    string? GetAccessToken();
    string? GetRefreshToken();
    string? GetUserFullName();

    void ClearSession();

    bool IsAuthenticated { get; }
}
