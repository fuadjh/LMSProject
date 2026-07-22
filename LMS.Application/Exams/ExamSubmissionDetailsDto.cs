using Common.Enums;

namespace LMS.Application.Exams;

public sealed record ExamSubmissionDetailsDto(
    Guid SubmissionId,
    string StudentFullName,
    string StudentNumber,
    int AttemptNumber,
    DateTime StartedAtUtc,
    DateTime? SubmittedAtUtc,
    decimal? TotalScore,
    IReadOnlyCollection<ExamSubmissionAnswerDto> Answers);

public sealed record ExamSubmissionAnswerDto(
    Guid AnswerId,
    Guid QuestionId,
    string QuestionTitle,
    QuestionType Type,
    decimal MaxScore,
    string? SelectedOptionText,
    string? EssayText,
    string? EssayAttachmentFileName,
    string? EssayAttachmentPath,
    decimal? AwardedScore);