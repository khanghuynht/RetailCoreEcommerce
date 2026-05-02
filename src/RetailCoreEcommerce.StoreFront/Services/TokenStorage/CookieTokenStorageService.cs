namespace RetailCoreEcommerce.StoreFront.Services.TokenStorage;

public class CookieTokenStorageService(IHttpContextAccessor httpContextAccessor) : ITokenStorageService
{
    private const string AccessTokenKey = "rc_access";
    private const string RefreshTokenKey = "rc_refresh";
    private const string UserNameKey = "rc_user";

    private HttpContext Context => httpContextAccessor.HttpContext!;

    public void StoreSession(string accessToken, string refreshToken, string fullName)
    {
        var secureOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        };

        // Display name cookie does not need to be HTTP-only — it is not a secret
        var displayOptions = new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        };

        Context.Response.Cookies.Append(AccessTokenKey, accessToken, secureOptions);
        Context.Response.Cookies.Append(RefreshTokenKey, refreshToken, secureOptions);
        Context.Response.Cookies.Append(UserNameKey, fullName, displayOptions);
    }

    public string? GetAccessToken() => Context.Request.Cookies[AccessTokenKey];
    public string? GetRefreshToken() => Context.Request.Cookies[RefreshTokenKey];
    public string? GetUserFullName() => Context.Request.Cookies[UserNameKey];

    public void ClearSession()
    {
        Context.Response.Cookies.Delete(AccessTokenKey);
        Context.Response.Cookies.Delete(RefreshTokenKey);
        Context.Response.Cookies.Delete(UserNameKey);
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(GetAccessToken());
}
