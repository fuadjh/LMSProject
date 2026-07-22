using Application.Common.Models;

namespace Application.Abstractions.Read;

public interface IUserAdminReadService
{
    Task<IReadOnlyCollection<UserLookupDto>> SearchUsersAsync(string? search, CancellationToken cancellationToken = default);
    Task<UserAccessDetailsDto?> GetUserAccessDetailsAsync(Guid userProfileId, CancellationToken cancellationToken = default);
}