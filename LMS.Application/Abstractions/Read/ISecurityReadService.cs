using Application.Common.Models;

namespace Application.Abstractions.Read;

public interface ISecurityReadService
{
    Task<IReadOnlyCollection<RoleListItemDto>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<string>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<string>> GetPermissionCatalogAsync(CancellationToken cancellationToken = default);
}