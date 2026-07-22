using Common.Enums;

namespace Common.Contracts.Academic;

public sealed record CourseListItemDto(
    Guid Id,
    Guid MajorId,
    string MajorTitle,
    string Title,
    string Code,
    int Units,
    CourseEnrollmentScope EnrollmentScope,
    bool IsActive);