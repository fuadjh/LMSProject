namespace Common.Contracts.Academic;

public sealed record SemesterListItemDto(
    Guid Id,
    string Title,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    bool IsActive);