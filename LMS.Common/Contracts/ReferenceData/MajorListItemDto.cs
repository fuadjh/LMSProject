namespace Common.Contracts.ReferenceData;

public sealed record MajorListItemDto(
    Guid Id,
    Guid FacultyId,
    string FacultyTitle,
    Guid UniversityId,
    string UniversityTitle,
    string Title,
    string Code,
    bool IsActive);