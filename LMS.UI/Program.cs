using Application.Security;
using Common.Security;
using LMS.UI.Auth;
using LMS.UI.Components;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using MudBlazor.Services;
using WebUi.Components.Pages.Admin.Academic.Courses;
using WebUi.Components.Shared.Crud;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<ICrudPageService<CourseListItemVm, CourseFormModel, Guid>, CourseApiClient>();
// Razor Components + Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// MVC Controllers for /auth/login and /auth/logout
builder.Services.AddControllers();

// MudBlazor
builder.Services.AddMudServices();

// Access to HttpContext inside handlers/controllers
builder.Services.AddHttpContextAccessor();

// Blazor auth state
builder.Services.AddCascadingAuthenticationState();


// -------------------------
// Configuration
// -------------------------
var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
if (string.IsNullOrWhiteSpace(apiBaseUrl))
    throw new InvalidOperationException("ApiBaseUrl is not configured in LMS.UI appsettings.");

apiBaseUrl = apiBaseUrl.TrimEnd('/') + "/";

var keysPath = builder.Configuration["DataProtection:KeysPath"];
if (string.IsNullOrWhiteSpace(keysPath))
{
    keysPath = Path.Combine(builder.Environment.ContentRootPath, "..", "shared-dp-keys");
}


// -------------------------
// Data Protection
// Must be shared with API if API also needs to read same auth cookie
// -------------------------
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.GetFullPath(keysPath)))
    .SetApplicationName(AuthenticationConstants.DataProtectionAppName);


// -------------------------
// Authentication / Authorization
// -------------------------
builder.Services.AddAuthentication(AuthenticationConstants.Scheme)
    .AddCookie(AuthenticationConstants.Scheme, options =>
    {
        options.Cookie.Name = AuthenticationConstants.CookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        options.LoginPath = "/login";
        options.LogoutPath = "/auth/logout";
        options.AccessDeniedPath = "/access-denied";

        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization(options =>
{
    foreach (var permission in Permissions.All)
    {
        options.AddPolicy(permission, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(CustomClaimTypes.Permission, permission);
        });
    }
});


// -------------------------
// Http Clients
// -------------------------

// For authenticated calls from UI server to API
builder.Services.AddScoped<ApiCookieForwardingHandler>();

builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<ApiCookieForwardingHandler>();

// For unauthenticated calls like login
builder.Services.AddHttpClient("ApiNoAuth", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Optional default resolved HttpClient => authenticated API client
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api"));

var app = builder.Build();


// -------------------------
// Middleware pipeline
// -------------------------
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