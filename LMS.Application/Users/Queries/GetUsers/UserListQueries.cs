using Application.Abstractions.Read;
using Application.Common.Models;
using Application.Common.Results;
using MediatR;

namespace Application.Users.Queries.GetUsers;

public sealed record GetStudentsQuery
    : PagedQuery,
      IRequest<Result<PagedResponse<UserProfileListItemDto>>>;

public sealed record GetInstructorsQuery
    : PagedQuery,
      IRequest<Result<PagedResponse<UserProfileListItemDto>>>;

public sealed record GetEducationExpertsQuery
    : PagedQuery,
      IRequest<Result<PagedResponse<UserProfileListItemDto>>>;

public sealed class UserListQueryHandler :
    IRequestHandler<
        GetStudentsQuery,
        Result<PagedResponse<UserProfileListItemDto>>>,
    IRequestHandler<
        GetInstructorsQuery,
        Result<PagedResponse<UserProfileListItemDto>>>,
    IRequestHandler<
        GetEducationExpertsQuery,
        Result<PagedResponse<UserProfileListItemDto>>>
{
    private readonly IUserAdminReadService _readService;

    public UserListQueryHandler(
        IUserAdminReadService readService)
    {
        _readService = readService;
    }

    public async Task<Result<PagedResponse<UserProfileListItemDto>>> Handle(
        GetStudentsQuery request,
        CancellationToken cancellationToken)
    {
        return await GetResultAsync(
            UserProfileType.Student,
            request,
            cancellationToken);
    }

    public async Task<Result<PagedResponse<UserProfileListItemDto>>> Handle(
        GetInstructorsQuery request,
        CancellationToken cancellationToken)
    {
        return await GetResultAsync(
            UserProfileType.Instructor,
            request,
            cancellationToken);
    }

    public async Task<Result<PagedResponse<UserProfileListItemDto>>> Handle(
        GetEducationExpertsQuery request,
        CancellationToken cancellationToken)
    {
        return await GetResultAsync(
            UserProfileType.EducationExpert,
            request,
            cancellationToken);
    }

    private async Task<Result<PagedResponse<UserProfileListItemDto>>>
        GetResultAsync(
            UserProfileType profileType,
            PagedQuery request,
            CancellationToken cancellationToken)
    {
        var response =
            await _readService.GetUsersAsync(
                profileType,
                request.Search,
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.SortDescending,
                cancellationToken);

        return Result<PagedResponse<UserProfileListItemDto>>
            .Success(response);
    }
}