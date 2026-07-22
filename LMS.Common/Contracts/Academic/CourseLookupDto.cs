namespace Common.Contracts.Academic;

public sealed record CourseLookupDto(
    Guid Id,
    string Title,
    string Code,
    int Units,
    Guid MajorId);