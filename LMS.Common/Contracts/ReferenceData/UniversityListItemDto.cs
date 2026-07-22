namespace Common.Contracts.ReferenceData;

public sealed record UniversityListItemDto(
    Guid Id,
    string Title,
    string Code,
     bool IsActive);