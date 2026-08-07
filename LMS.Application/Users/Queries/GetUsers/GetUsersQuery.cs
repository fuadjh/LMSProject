using Application.Common.Models;
using Common.Enums;
using MediatR;

namespace Application.Users.Queries.GetUsers;

public sealed record GetUsersQuery(
    UserRoleType RoleType,
    string? Search,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    bool SortDescending = false)
    : IRequest<PagedResponse<UserListItemDto>>;