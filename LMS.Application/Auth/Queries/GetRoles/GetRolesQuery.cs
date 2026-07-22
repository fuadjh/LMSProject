using Application.Common.Models;
using Application.Common.Results;
using MediatR;

namespace Application.Security.Queries.GetRoles;

public sealed record GetRolesQuery
    : IRequest<Result<IReadOnlyCollection<RoleListItemDto>>>;