using Common.Enums;


namespace Common.Contracts.Exams;

public sealed record ExamLookupDto(
    Guid ExamId,
    string Title,
    ExamStatus Status,
    ExamDeliveryMode DeliveryMode,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    int DurationMinutes,
    int MaxAttemptsPerStudent);