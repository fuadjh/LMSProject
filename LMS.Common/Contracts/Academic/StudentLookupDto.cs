namespace Common.Contracts.Academic;

public sealed record StudentLookupDto(
    Guid StudentProfileId,
    Guid UserProfileId,
    string FullName,
    string StudentNumber,
    Guid MajorId);