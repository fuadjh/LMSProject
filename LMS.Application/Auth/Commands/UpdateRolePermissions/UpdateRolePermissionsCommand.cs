using Application.Common.Results;
using MediatR;

namespace Application.Security.Commands.UpdateRolePermissions;

public sealed record UpdateRolePermissionsCommand(
    Guid RoleId,
    IReadOnlyCollection<string> Permissions) : IRequest<Result>;