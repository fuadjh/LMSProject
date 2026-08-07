namespace Common.Contracts.Academic;

public sealed record StudentLookupDto(
    Guid StudentProfileId,
    Guid UserId,
    string FullName,
    string StudentNumber,
    Guid MajorId);