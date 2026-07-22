namespace Application.Common.Models;

public sealed record UserLookupDto(
    Guid UserProfileId,
    Guid AuthUserId,
    string FullName,
    string UserName,
    bool IsActive);