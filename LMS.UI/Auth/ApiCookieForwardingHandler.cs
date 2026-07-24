using Common.Security;
using Microsoft.AspNetCore.Http;

namespace LMS.UI.Auth;

public sealed class ApiCookieForwardingHandler
    : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ApiCookieForwardingHandler> _logger;

    public ApiCookieForwardingHandler(
        IHttpContextAccessor httpContextAccessor,
        ILogger<ApiCookieForwardingHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext =
            _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            _logger.LogWarning(
                "HttpContext is null while forwarding API cookie. Request: {RequestUri}",
                request.RequestUri);
        }
        else if (httpContext.Request.Cookies.TryGetValue(
                     AuthenticationConstants.CookieName,
                     out var cookieValue) &&
                 !string.IsNullOrWhiteSpace(cookieValue))
        {
            request.Headers.Remove("Cookie");

            request.Headers.TryAddWithoutValidation(
                "Cookie",
                $"{AuthenticationConstants.CookieName}={cookieValue}");

            _logger.LogDebug(
                "Authentication cookie forwarded to API. Request: {RequestUri}",
                request.RequestUri);
        }
        else
        {
            _logger.LogWarning(
                "Authentication cookie was not found. Request: {RequestUri}",
                request.RequestUri);
        }

        var response = await base.SendAsync(
            request,
            cancellationToken);

        if (response.StatusCode is
            System.Net.HttpStatusCode.Unauthorized or
            System.Net.HttpStatusCode.Forbidden)
        {
            _logger.LogWarning(
                "API authorization failed. StatusCode: {StatusCode}, Request: {RequestUri}",
                response.StatusCode,
                request.RequestUri);
        }

        return response;
    }
}