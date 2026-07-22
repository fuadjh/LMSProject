using System.Security.Claims;
using Application.Abstractions.Auth;
using Common.Security;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services;

public sealed class RolePermissionService : IRolePermissionService
{
    private readonly RoleManager<ApplicationRole> _roleManager;

    public RolePermissionService(RoleManager<ApplicationRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<Guid> CreateRoleAsync(string roleName, string? description, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new InvalidOperationException("Role name is required.");

        if (await _roleManager.RoleExistsAsync(roleName))
            throw new InvalidOperationException("Role already exists.");

        var role = new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = roleName.Trim(),
            Description = description
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(" | ", result.Errors.Select(x => x.Description)));

        return role.Id;
    }

    public async Task UpdateRolePermissionsAsync(
        Guid roleId,
        IEnumerable<string> permissions,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString())
                   ?? throw new InvalidOperationException("Role not found.");

        var desired = permissions
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var invalidPermissions = desired
            .Except(Permissions.All, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (invalidPermissions.Any())
            throw new InvalidOperationException("One or more permissions are invalid.");

        var currentClaims = await _roleManager.GetClaimsAsync(role);
        var currentPermissions = currentClaims
            .Where(x => x.Type == CustomClaimTypes.Permission)
            .Select(x => x.Value)
            .ToArray();

        var toRemove = currentPermissions.Except(desired, StringComparer.OrdinalIgnoreCase).ToArray();
        var toAdd = desired.Except(currentPermissions, StringComparer.OrdinalIgnoreCase).ToArray();

        foreach (var permission in toRemove)
        {
            var result = await _roleManager.RemoveClaimAsync(
                role,
                new Claim(CustomClaimTypes.Permission, permission));

            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(" | ", result.Errors.Select(x => x.Description)));
        }

        foreach (var permission in toAdd)
        {
            var result = await _roleManager.AddClaimAsync(
                role,
                new Claim(CustomClaimTypes.Permission, permission));

            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(" | ", result.Errors.Select(x => x.Description)));
        }
    }

    public async Task<IReadOnlyCollection<string>> GetRolePermissionsAsync(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString())
                   ?? throw new InvalidOperationException("Role not found.");

        var claims = await _roleManager.GetClaimsAsync(role);

        return claims
            .Where(x => x.Type == CustomClaimTypes.Permission)
            .Select(x => x.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}