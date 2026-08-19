using Application.Abstractions.Read;
using Application.Common.Models;
using Common.Enums;
using MediatR;

namespace Application.Users.Queries;

public sealed record GetUsersQuery(
    UserRoleType Role,
    string? Search,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    bool Descending = false)
    : IRequest<PagedResponse<UserListItemDto>>;

public sealed class GetUsersQueryHandler
    : IRequestHandler<
        GetUsersQuery,
        PagedResponse<UserListItemDto>>
{
    private readonly IUserAdminReadService _readService;

    public GetUsersQueryHandler(
        IUserAdminReadService readService)
    {
        _readService = readService;
    }

    public Task<PagedResponse<UserListItemDto>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        return _readService.GetUsersAsync(
            request.Role,
            request.Search,
            request.Page,
            request.PageSize,
            request.SortBy,
            request.Descending,
            cancellationToken);
    }
}

public sealed record GetUserDetailsQuery(
    Guid UserId,
    UserRoleType Role)
    : IRequest<UserDetailsDto?>;

public sealed class GetUserDetailsQueryHandler
    : IRequestHandler<
        GetUserDetailsQuery,
        UserDetailsDto?>
{
    private readonly IUserAdminReadService _readService;

    public GetUserDetailsQueryHandler(
        IUserAdminReadService readService)
    {
        _readService = readService;
    }

    public Task<UserDetailsDto?> Handle(
        GetUserDetailsQuery request,
        CancellationToken cancellationToken)
    {
        return _readService.GetUserDetailsAsync(
            request.UserId,
            request.Role,
            cancellationToken);
    }
}