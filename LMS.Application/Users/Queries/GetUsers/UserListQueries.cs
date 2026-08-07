using Application.Abstractions.Read;
using Application.Common.Models;
using Application.Common.Results;
using Common.Enums;
using MediatR;

namespace Application.Users.Queries.GetUsers;

public sealed record GetStudentsQuery
    : PagedQuery,
      IRequest<Result<PagedResponse<UserListItemDto>>>;

public sealed record GetInstructorsQuery
    : PagedQuery,
      IRequest<Result<PagedResponse<UserListItemDto>>>;

public sealed record GetEducationExpertsQuery
    : PagedQuery,
      IRequest<Result<PagedResponse<UserListItemDto>>>;

public sealed class UserListQueryHandler :
    IRequestHandler<
        GetStudentsQuery,
        Result<PagedResponse<UserListItemDto>>>,
    IRequestHandler<
        GetInstructorsQuery,
        Result<PagedResponse<UserListItemDto>>>,
    IRequestHandler<
        GetEducationExpertsQuery,
        Result<PagedResponse<UserListItemDto>>>
{
    private readonly IUserAdminReadService _readService;

    public UserListQueryHandler(
        IUserAdminReadService readService)
    {
        _readService = readService;
    }

    public async Task<Result<PagedResponse<UserListItemDto>>> Handle(
        GetStudentsQuery request,
        CancellationToken cancellationToken)
    {
        return await GetResultAsync(
            UserRoleType.Student,
            request,
            cancellationToken);
    }

    public async Task<Result<PagedResponse<UserListItemDto>>> Handle(
        GetInstructorsQuery request,
        CancellationToken cancellationToken)
    {
        return await GetResultAsync(
            UserRoleType.Instructor,
            request,
            cancellationToken);
    }

    public async Task<Result<PagedResponse<UserListItemDto>>> Handle(
        GetEducationExpertsQuery request,
        CancellationToken cancellationToken)
    {
        return await GetResultAsync(
            UserRoleType.Expert,
            request,
            cancellationToken);
    }

    private async Task<Result<PagedResponse<UserListItemDto>>>
        GetResultAsync(
            UserRoleType roleType,
            PagedQuery request,
            CancellationToken cancellationToken)
    {
        var response =
            await _readService.GetUsersAsync(
                roleType,
                request.Search,
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.SortDescending,
                cancellationToken);

        return Result<PagedResponse<UserListItemDto>>
            .Success(response);
    }
}