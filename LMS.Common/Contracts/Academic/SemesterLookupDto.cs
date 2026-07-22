namespace Common.Contracts.Academic;

public sealed record SemesterLookupDto(
    Guid Id,
    string Title,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc);