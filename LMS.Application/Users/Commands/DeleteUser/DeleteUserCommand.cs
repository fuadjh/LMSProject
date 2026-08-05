using Application.Common.Results;
using MediatR;

namespace Application.Users.Commands.DeleteUser;

public sealed record DeleteUserCommand(
    Guid UserProfileId) : IRequest<Result>;