using Application.Abstractions.Read;
using Application.Common.Models;
using Application.Common.Results;
using MediatR;

namespace Application.Users.Queries.SearchUsers;

public sealed class SearchUsersQueryHandler
    : IRequestHandler<SearchUsersQuery, Result<IReadOnlyCollection<UserLookupDto>>>
{
    private readonly IUserAdminReadService _userAdminReadService;

    public SearchUsersQueryHandler(IUserAdminReadService userAdminReadService)
    {
        _userAdminReadService = userAdminReadService;
    }

    public async Task<Result<IReadOnlyCollection<UserLookupDto>>> Handle(
        SearchUsersQuery request,
        CancellationToken cancellationToken)
    {
        var users = await _userAdminReadService.SearchUsersAsync(
            request.Search,
            cancellationToken);

        return Result<IReadOnlyCollection<UserLookupDto>>.Success(users);
    }
}