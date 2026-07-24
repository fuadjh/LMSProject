namespace Common.Contracts.Academic;

public sealed record CourseOfferingLookupDto(
    Guid Id,
    Guid CourseId,
    Guid SemesterId,
    Guid MajorId,
    string CourseTitle,
    string SemesterTitle,
    string? InstructorName,
    Guid? InstructorProfileId,
    string SectionCode,
    int Capacity,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    bool IsActive);