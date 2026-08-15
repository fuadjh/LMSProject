using Application.Abstractions.Read;
using Application.Common.Models;
using Application.Common.Results;
using MediatR;

namespace Application.Users.Queries.SearchUsers;

public sealed record SearchUsersQuery(
    string? Search)
    : IRequest<Result<IReadOnlyCollection<UserLookupDto>>>;

public sealed class SearchUsersQueryHandler
    : IRequestHandler<
        SearchUsersQuery,
        Result<IReadOnlyCollection<UserLookupDto>>>
{
    private readonly IUserAdminReadService _readService;

    public SearchUsersQueryHandler(
        IUserAdminReadService readService)
    {
        _readService = readService;
    }

    public async Task<Result<IReadOnlyCollection<UserLookupDto>>> Handle(
        SearchUsersQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _readService.SearchUsersAsync(
            request.Search,
            cancellationToken);

        return Result<IReadOnlyCollection<UserLookupDto>>
            .Success(result);
    }
}