using Common.Security;


namespace LMS.UI.Auth;

public sealed class ApiCookieForwardingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiCookieForwardingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext;

        if (context is not null &&
            context.Request.Cookies.TryGetValue(AuthenticationConstants.CookieName, out var cookieValue) &&
            !string.IsNullOrWhiteSpace(cookieValue))
        {
            request.Headers.Remove("Cookie");
            request.Headers.Add("Cookie", $"{AuthenticationConstants.CookieName}={cookieValue}");
        }

        return base.SendAsync(request, cancellationToken);
    }
}