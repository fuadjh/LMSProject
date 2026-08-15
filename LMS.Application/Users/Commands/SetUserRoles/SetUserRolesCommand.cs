using Application.Common.Models;
using MediatR;

namespace Application.Users.Commands.SetUserRoles;

public sealed record SetUserRolesCommand(
    Guid UserId,
    IReadOnlyCollection<string> Roles)
    : IRequest<IdentityOperationResult>;