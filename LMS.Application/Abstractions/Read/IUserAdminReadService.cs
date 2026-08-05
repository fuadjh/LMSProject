using Application.Common.Models;

namespace Application.Abstractions.Read;

public interface IUserAdminReadService
{
    Task<IReadOnlyCollection<UserLookupDto>> SearchUsersAsync(
        string? search,
        CancellationToken cancellationToken = default);

    Task<UserAccessDetailsDto?> GetUserAccessDetailsAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<UserProfileListItemDto>> GetUsersAsync(
        UserProfileType profileType,
        string? search,
        int pageNumber,
        int pageSize,
        string? sortBy,
        bool sortDescending,
        CancellationToken cancellationToken = default);

    Task<UserProfileDetailsDto?> GetUserProfileDetailsAsync(
        Guid userProfileId,
        UserProfileType profileType,
        CancellationToken cancellationToken = default);

    Task<UserByNationalCodeDto?> GetUserByNationalCodeAsync(
        string nationalCode,
        CancellationToken cancellationToken = default);
}