using System.Security.Claims;
using Application.Abstractions.Auth;
using Common.Security;
using Microsoft.AspNetCore.Http;

namespace WebApi.Services;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public Guid? AuthUserId =>
        Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : null;

    public Guid? UserId =>
        Guid.TryParse(User?.FindFirstValue(CustomClaimTypes.UserId), out var id)
            ? id
            : null;

    public string? UserName => User?.FindFirstValue(ClaimTypes.Name);

    public IReadOnlyCollection<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(x => x.Value).ToArray()
        ?? Array.Empty<string>();

    public bool HasPermission(string permission) =>
        User?.Claims.Any(x =>
            x.Type == CustomClaimTypes.Permission &&
            x.Value.Equals(permission, StringComparison.OrdinalIgnoreCase)) == true;
}