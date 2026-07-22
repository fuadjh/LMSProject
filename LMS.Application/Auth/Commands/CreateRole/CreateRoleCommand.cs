using Application.Common.Results;
using MediatR;

namespace Application.Security.Commands.CreateRole;

public sealed record CreateRoleCommand(
    string Name,
    string? Description) : IRequest<Result<Guid>>;