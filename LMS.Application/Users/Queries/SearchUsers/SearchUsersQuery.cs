using Application.Common.Models;
using Application.Common.Results;
using MediatR;

namespace Application.Users.Queries.SearchUsers;

public sealed record SearchUsersQuery(
    string? Search) : IRequest<Result<IReadOnlyCollection<UserLookupDto>>>;