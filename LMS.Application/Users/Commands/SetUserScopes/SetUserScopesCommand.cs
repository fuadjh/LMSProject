using Application.Common.Results;
using MediatR;

namespace Application.Users.Commands.SetUserScopes;

public sealed record SetUserScopesCommand(
    Guid UserProfileId,
    IReadOnlyCollection<Guid> FacultyIds,
    IReadOnlyCollection<Guid> MajorIds) : IRequest<Result>;