using Application.Common.Results;
using MediatR;

namespace Application.Users.Commands.SetUserActiveStatus;

public sealed record SetUserActiveStatusCommand(
    Guid UserProfileId,
    bool IsActive) : IRequest<Result>;