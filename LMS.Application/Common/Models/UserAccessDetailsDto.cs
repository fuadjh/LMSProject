namespace Application.Common.Models;

public sealed record UserAccessDetailsDto(
    Guid UserProfileId,
    Guid AuthUserId,
    string FullName,
    string UserName,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<Guid> FacultyIds,
    IReadOnlyCollection<Guid> MajorIds);