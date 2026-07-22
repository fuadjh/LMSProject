using Common.Security;
using System.Security.Claims;

namespace WebUi.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool HasPermission(this ClaimsPrincipal? user, string permission)
    {
        if (user is null)
            return false;

        return user.Claims.Any(c =>
            c.Type == CustomClaimTypes.Permission &&
            c.Value == permission);
    }
}