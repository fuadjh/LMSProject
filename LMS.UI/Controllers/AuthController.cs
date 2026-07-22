using System.Net.Http.Json;
using System.Security.Claims;
using Common.Contracts.Auth;
using Common.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.UI.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Route("auth")]
public sealed class AuthController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IHttpClientFactory httpClientFactory, ILogger<AuthController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Login([FromForm] LoginFormRequest request, CancellationToken cancellationToken)
    {
        var returnUrl = NormalizeReturnUrl(request.ReturnUrl, "/");

        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
            return RedirectToLoginWithError("نام کاربری و کلمه عبور الزامی است.", returnUrl);

        try
        {
            var client = _httpClientFactory.CreateClient("ApiNoAuth");

            var dto = new LoginRequestDto
            {
                UserName = request.UserName.Trim(),
                Password = request.Password
            };

            _logger.LogInformation("Sending login request for user {UserName} to API.", dto.UserName);

            var apiResponse = await client.PostAsJsonAsync("api/auth/login", dto, cancellationToken);

            var responseText = await apiResponse.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("Login API response status: {StatusCode}", apiResponse.StatusCode);
            _logger.LogInformation("Login API response body: {ResponseBody}", responseText);

            if (!apiResponse.IsSuccessStatusCode)
            {
                return RedirectToLoginWithError(
                    $"خطا در ورود از API. Status: {(int)apiResponse.StatusCode}. Response: {responseText}",
                    returnUrl);
            }

            var signInData = await apiResponse.Content.ReadFromJsonAsync<SignInDataDto>(cancellationToken: cancellationToken);

            if (signInData is null)
                return RedirectToLoginWithError("پاسخ نامعتبر از سرویس احراز هویت دریافت شد.", returnUrl);

            var claims = BuildClaims(signInData);
            var identity = new ClaimsIdentity(claims, AuthenticationConstants.Scheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = request.RememberMe,
                AllowRefresh = true
            };

            if (request.RememberMe)
                authProperties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30);

            await HttpContext.SignInAsync(
                AuthenticationConstants.Scheme,
                principal,
                authProperties);

            return LocalRedirect(returnUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while logging in from UI.");
            return RedirectToLoginWithError($"خطای سیستمی در ورود: {ex.Message}", returnUrl);
        }
    }

    [HttpPost("logout")]
    [Authorize]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Logout([FromForm] string? returnUrl = "/login")
    {
        await HttpContext.SignOutAsync(AuthenticationConstants.Scheme);
        return LocalRedirect(NormalizeReturnUrl(returnUrl, "/login"));
    }

    private static List<Claim> BuildClaims(SignInDataDto signInData)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, signInData.AuthUserId.ToString()),
            new(ClaimTypes.Name, signInData.UserName)
        };

        if (signInData.UserProfileId.HasValue)
            claims.Add(new Claim(CustomClaimTypes.UserProfileId, signInData.UserProfileId.Value.ToString()));

        if (!string.IsNullOrWhiteSpace(signInData.DisplayName))
            claims.Add(new Claim(ClaimTypes.GivenName, signInData.DisplayName));

        foreach (var role in signInData.Roles.Distinct(StringComparer.OrdinalIgnoreCase))
            claims.Add(new Claim(ClaimTypes.Role, role));

        foreach (var permission in signInData.Permissions.Distinct(StringComparer.OrdinalIgnoreCase))
            claims.Add(new Claim(CustomClaimTypes.Permission, permission));

        return claims;
    }

    private IActionResult RedirectToLoginWithError(string message, string returnUrl)
    {
        var encodedMessage = Uri.EscapeDataString(message);
        var encodedReturnUrl = Uri.EscapeDataString(returnUrl);
        return Redirect($"/login?error={encodedMessage}&returnUrl={encodedReturnUrl}");
    }

    private static string NormalizeReturnUrl(string? returnUrl, string defaultValue = "/")
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            return defaultValue;

        if (!returnUrl.StartsWith("/"))
            return defaultValue;

        if (returnUrl.StartsWith("//"))
            return defaultValue;

        return returnUrl;
    }

    public sealed class LoginFormRequest
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }
}