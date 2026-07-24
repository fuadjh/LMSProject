using Common.Enums;

namespace Common.Contracts.Academic;

public sealed record CourseManageDto(
    Guid Id,
    Guid MajorId,
    string MajorTitle,
    string Title,
    string Code,
    int Units,
    CourseEnrollmentScope EnrollmentScope,
    bool IsActive);

public sealed record SemesterManageDto(
    Guid Id,
    string Title,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    bool IsActive);

public sealed record CourseOfferingManageDto(
    Guid Id,
    Guid CourseId,
    string CourseTitle,
    Guid MajorId,
    string MajorTitle,
    Guid SemesterId,
    string SemesterTitle,
    string SectionCode,
    int Capacity,
    int ActiveEnrollmentCount,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    Guid? InstructorProfileId,
    string? InstructorName,
    bool IsActive);