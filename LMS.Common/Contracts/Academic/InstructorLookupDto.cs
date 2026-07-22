namespace Common.Contracts.Academic;

public sealed record InstructorLookupDto(
    Guid InstructorProfileId,
    Guid UserProfileId,
    string FullName,
    string PersonnelCode);