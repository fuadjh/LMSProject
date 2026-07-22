using Application.Abstractions.Read;
using Application.Common.Models;
using Common.Security;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class SecurityReadService : ISecurityReadService
{
    private readonly RoleManager<ApplicationRole> _roleManager;

    public SecurityReadService(RoleManager<ApplicationRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<IReadOnlyCollection<RoleListItemDto>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _roleManager.Roles
            .OrderBy(x => x.Name)
            .Select(x => new RoleListItemDto(x.Id, x.Name!, x.Description))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString())
                   ?? throw new InvalidOperationException("Role not found.");

        var claims = await _roleManager.GetClaimsAsync(role);

        return claims
            .Where(x => x.Type == CustomClaimTypes.Permission)
            .Select(x => x.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToArray();
    }

    public Task<IReadOnlyCollection<string>> GetPermissionCatalogAsync(CancellationToken cancellationToken = default)
        => Task.FromResult((IReadOnlyCollection<string>)Permissions.All.OrderBy(x => x).ToArray());
}