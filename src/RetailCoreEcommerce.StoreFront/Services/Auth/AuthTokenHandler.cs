using System.Net.Http.Headers;

namespace RetailCoreEcommerce.StoreFront.Services.Auth;

/// <summary>
/// DelegatingHandler that automatically attaches the Bearer token from the
/// cookie session to every outbound HTTP request made by typed HttpClients.
/// </summary>
public class AuthTokenHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = httpContextAccessor.HttpContext?.Request.Cookies["rc_access"];

        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, cancellationToken);
    }
}
