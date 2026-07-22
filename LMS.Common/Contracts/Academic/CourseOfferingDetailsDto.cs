using Common.Enums;

namespace Common.Contracts.Academic;

public sealed record CourseOfferingDetailsDto(
    Guid Id,
    Guid CourseId,
    Guid SemesterId,
    Guid MajorId,
    string MajorTitle,
    string CourseTitle,
    string CourseCode,
    string SemesterTitle,
    string SectionCode,
    int Capacity,
    int ActiveEnrollmentCount,
    CourseEnrollmentScope EnrollmentScope,
    Guid? InstructorProfileId,
    string? InstructorName,
    bool IsActive);