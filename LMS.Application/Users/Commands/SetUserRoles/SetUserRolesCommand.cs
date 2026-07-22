using Application.Common.Results;
using MediatR;

namespace Application.Users.Commands.SetUserRoles;

public sealed record SetUserRolesCommand(
    Guid AuthUserId,
    IReadOnlyCollection<string> Roles): IRequest<Result>;