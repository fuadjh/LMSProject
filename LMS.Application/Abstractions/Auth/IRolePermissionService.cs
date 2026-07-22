namespace Application.Abstractions.Auth;

public interface IRolePermissionService
{
    Task<Guid> CreateRoleAsync(string roleName, string? description, CancellationToken cancellationToken = default);

    Task UpdateRolePermissionsAsync(
        Guid roleId,
        IEnumerable<string> permissions,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<string>> GetRolePermissionsAsync(
        Guid roleId,
        CancellationToken cancellationToken = default);
}