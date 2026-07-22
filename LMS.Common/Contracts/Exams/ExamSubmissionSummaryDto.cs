namespace Common.Contracts.Exams;

public sealed record ExamSubmissionSummaryDto(
    Guid SubmissionId,
    string StudentFullName,
    string StudentNumber,
    int AttemptNumber,
    DateTime StartedAtUtc,
    DateTime? SubmittedAtUtc,
    decimal? TotalScore);