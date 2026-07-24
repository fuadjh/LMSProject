namespace Common.Contracts.Academic;

public sealed record CourseOfferingDetailsDto(
    Guid Id,
    Guid CourseId,
    Guid SemesterId,
    Guid MajorId,
    string CourseTitle,
    string CourseCode,
    int CourseUnits,
    string SemesterTitle,
    Guid? InstructorProfileId,
    string? InstructorName,
    string SectionCode,
    int Capacity,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    bool IsActive);