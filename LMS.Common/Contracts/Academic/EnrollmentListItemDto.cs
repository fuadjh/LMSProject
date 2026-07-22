namespace Common.Contracts.Academic;

public sealed record EnrollmentListItemDto(
    Guid EnrollmentId,
    Guid StudentProfileId,
    string StudentFullName,
    string StudentNumber,
    bool IsActive,
    DateTime EnrolledAtUtc);