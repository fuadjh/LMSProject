namespace Common.Contracts.Academic;

public sealed record InstructorLookupDto(
    Guid InstructorProfileId,
    Guid UserId,
    string FullName,
    string PersonnelCode);