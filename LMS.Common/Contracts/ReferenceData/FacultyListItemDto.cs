namespace Common.Contracts.ReferenceData;

public sealed record FacultyListItemDto(
    Guid Id,
    Guid UniversityId,
    string UniversityTitle,
    string Title,
    string Code,
    bool IsActive);