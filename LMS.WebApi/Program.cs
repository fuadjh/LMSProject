using Application.Abstractions.Auth;
using Application.Abstractions.Identity;
using Application.Abstractions.Learning;
using Application.Abstractions.Persistence;
using Application.Abstractions.Read;
using Application.Abstractions.Security;
using Application.Common.Behaviors;
using Common.Security;
using FluentValidation;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Infrastructure.Services;
using LMS.Application.Auth.Commands.Login;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApi.Authorization;
using WebApi.Middlewares;
using WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit =
        500L * 1024L * 1024L;
});

builder.Services.AddDbContext<LmsDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<LmsDbContext>()
    
    .AddDefaultTokenProviders();

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

            options.Events =
                new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode =
                            StatusCodes.Status401Unauthorized;

                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode =
                            StatusCodes.Status403Forbidden;

                        return Task.CompletedTask;
                    }
                };
        });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<
    IAuthorizationPolicyProvider,
    PermissionPolicyProvider>();
builder.Services.AddScoped<
    IUserAccountService,
    UserAccountService>();
builder.Services.AddScoped<
    IAuthorizationHandler,
    PermissionAuthorizationHandler>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddMediatR(
    typeof(LoginCommandHandler).Assembly);

builder.Services.AddValidatorsFromAssembly(
    typeof(LoginCommandHandler).Assembly);

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));

builder.Services.AddScoped<IApplicationDbContext>(
    provider =>
        provider.GetRequiredService<LmsDbContext>());

builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IUserAccountService, UserAccountService>();
builder.Services.AddScoped<
    IRolePermissionService,
    RolePermissionService>();

builder.Services.AddScoped<
    IAccessScopeService,
    AccessScopeService>();

builder.Services.AddScoped<
    ILearningAccessService,
    LearningAccessService>();

builder.Services.AddScoped<
    IHtmlContentSanitizer,
    HtmlContentSanitizer>();

builder.Services.AddScoped<
    IPrivateFileStorage,
    LocalPrivateFileStorage>();

builder.Services.AddScoped<
    IReferenceDataReadService,
    ReferenceDataReadService>();

builder.Services.AddScoped<
    ISecurityReadService,
    SecurityReadService>();

builder.Services.AddScoped<
    IUserAdminReadService,
    UserAdminReadService>();

builder.Services.AddScoped<
    IAcademicReadService,
    AcademicReadService>();
builder.Services.AddScoped<
    IVideoTranscoder,
    FfmpegVideoTranscoder>();
builder.Services.AddTransient<
    ExceptionHandlingMiddleware>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ui", policy =>
    {
        policy
            .WithOrigins("https://localhost:7002")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("ui");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await IdentitySeeder.SeedAsync(app.Services);

app.Run();