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
    : IRequest<PagedResponse<UserProfileListItemDto>>;

public sealed class GetUsersQueryHandler(
    IUserAdminReadService readService)
    : IRequestHandler<
        GetUsersQuery,
        PagedResponse<UserProfileListItemDto>>
{
    public Task<PagedResponse<UserProfileListItemDto>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken) =>
        readService.GetUsersAsync(
            request.Role,
            request.Search,
            request.Page,
            request.PageSize,
            request.SortBy,
            request.Descending,
            cancellationToken);
}

public sealed record GetUserDetailsQuery(
    Guid UserId,
    UserRoleType Role)
    : IRequest<UserProfileDetailsDto?>;

public sealed class GetUserDetailsQueryHandler(
    IUserAdminReadService readService)
    : IRequestHandler<
        GetUserDetailsQuery,
        UserProfileDetailsDto?>
{
    public Task<UserProfileDetailsDto?> Handle(
        GetUserDetailsQuery request,
        CancellationToken cancellationToken) =>
        readService.GetUserDetailsAsync(
            request.UserId,
            request.Role,
            cancellationToken);
}