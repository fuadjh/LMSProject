using Application.Common.Results;
using MediatR;

namespace Application.Security.Queries.GetRolePermissions;

public sealed record GetRolePermissionsQuery(
    Guid RoleId) : IRequest<Result<IReadOnlyCollection<string>>>;