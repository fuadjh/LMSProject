using Common.Enums;

namespace Common.Contracts.Academic;

public sealed record CourseListItemDto(
    Guid Id, Guid MajorId, string MajorTitle, string Title,
    string Code, int Units, CourseEnrollmentScope EnrollmentScope,
    bool IsActive);

public sealed record SemesterListItemDto(
    Guid Id, string Title, DateTime StartsAtUtc,
    DateTime EndsAtUtc, bool IsActive);

public sealed record CourseOfferingListItemDto(
    Guid Id, Guid CourseId, Guid SemesterId, Guid MajorId,
    string MajorTitle, string CourseTitle, string CourseCode,
    string SemesterTitle, string SectionCode, int Capacity,
    int ActiveEnrollmentCount, CourseEnrollmentScope EnrollmentScope,
    Guid? InstructorProfileId, string? InstructorName, bool IsActive);
