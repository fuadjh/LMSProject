namespace Common.Contracts.Academic;

public sealed record CourseOfferingLookupDto(
    Guid Id, Guid CourseId, Guid SemesterId, Guid MajorId,
    string CourseTitle, string SemesterTitle, string SectionCode,
    int Capacity, string? InstructorName, Guid? InstructorProfileId);
