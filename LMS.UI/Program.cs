using Common.Security;
using LMS.UI.Auth;
using LMS.UI.Components;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using MudBlazor.Services;
using WebUi.Components.Pages.Admin.Academic.Courses;
using WebUi.Components.Pages.Admin.Users;
using WebUi.Components.Shared.Crud;
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();

builder.Services.AddMudServices();

builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();

var apiBaseUrl =
    builder.Configuration["ApiBaseUrl"];

if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
    throw new InvalidOperationException(
        "ApiBaseUrl is not configured.");
}

apiBaseUrl = apiBaseUrl.TrimEnd('/') + "/";

var keysPath =
    builder.Configuration["DataProtection:KeysPath"];

if (string.IsNullOrWhiteSpace(keysPath))
{
    keysPath = Path.Combine(
        builder.Environment.ContentRootPath,
        "..",
        "shared-dp-keys");
}

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(
        new DirectoryInfo(
            Path.GetFullPath(keysPath)))
    .SetApplicationName(
        AuthenticationConstants.DataProtectionAppName);

builder.Services
    .AddAuthentication(AuthenticationConstants.Scheme)
    .AddCookie(
        AuthenticationConstants.Scheme,
        options =>
        {
            options.Cookie.Name =
                AuthenticationConstants.CookieName;

            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy =
                CookieSecurePolicy.Always;

            options.LoginPath = "/login";
            options.LogoutPath = "/auth/logout";
            options.AccessDeniedPath = "/access-denied";

            options.SlidingExpiration = true;
            options.ExpireTimeSpan =
                TimeSpan.FromHours(8);
        });

builder.Services.AddAuthorization(options =>
{
    foreach (var permission in Permissions.All)
    {
        options.AddPolicy(
            permission,
            policy =>
            {
                policy.RequireAuthenticatedUser();

                policy.RequireClaim(
                    CustomClaimTypes.Permission,
                    permission);
            });
    }
});

builder.Services.AddScoped<
    ApiCookieForwardingHandler>();
builder.Services.AddScoped<UserManagementApiClient>();
builder.Services
    .AddHttpClient(
        "Api",
        client =>
        {
            client.BaseAddress =
                new Uri(apiBaseUrl);

            client.Timeout =
                TimeSpan.FromMinutes(30);
        })
    .AddHttpMessageHandler<
        ApiCookieForwardingHandler>();

builder.Services.AddHttpClient(
    "ApiNoAuth",
    client =>
    {
        client.BaseAddress =
            new Uri(apiBaseUrl);
    });

builder.Services.AddScoped(provider =>
    provider
        .GetRequiredService<IHttpClientFactory>()
        .CreateClient("Api"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();