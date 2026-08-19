using Application.Common.Results;
using Common.Enums;
using MediatR;

namespace Application.Users.Commands.SetUserScopes;

public sealed record SetUserScopesCommand(
    Guid UserId,
    UserRoleType RoleType,
    IReadOnlyCollection<Guid> FacultyIds,
    IReadOnlyCollection<Guid> MajorIds)
    : IRequest<Result>;