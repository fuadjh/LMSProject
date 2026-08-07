using Application.Common.Models;
using Common.Enums;

namespace Application.Abstractions.Read;

public interface IUserAdminReadService
{
    Task<IReadOnlyCollection<UserLookupDto>> SearchUsersAsync(
        string? search,
        CancellationToken cancellationToken = default);

    Task<UserAccessDetailsDto?> GetUserAccessDetailsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<UserListItemDto>> GetUsersAsync(
        UserRoleType roleType,
        string? search,
        int pageNumber,
        int pageSize,
        string? sortBy,
        bool sortDescending,
        CancellationToken cancellationToken = default);

    Task<UserDetailsDto?> GetUserDetailsAsync(
        Guid userId,
        UserRoleType roleType,
        CancellationToken cancellationToken = default);

    Task<UserByNationalCodeDto?> GetUserByNationalCodeAsync(
        string nationalCode,
        CancellationToken cancellationToken = default);
}