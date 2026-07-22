namespace Common.Contracts.Academic;

public sealed record CourseOfferingDetailsDto(
    Guid Id,
    Guid CourseId,
    Guid SemesterId,
    Guid MajorId,
    string CourseTitle,
    string SemesterTitle,
    Guid? InstructorProfileId,
    string? InstructorName);