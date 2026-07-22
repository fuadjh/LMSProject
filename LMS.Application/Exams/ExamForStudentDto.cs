using Common.Enums;


namespace LMS.Application.Exams;

public sealed record ExamForStudentDto(
    Guid ExamId,
    string Title,
    ExamDeliveryMode DeliveryMode,
    int DurationMinutes,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    IReadOnlyCollection<ExamForStudentQuestionDto> Questions);

public sealed record ExamForStudentQuestionDto(
    Guid QuestionId,
    string Title,
    string Body,
    QuestionType Type,
    decimal Score,
    string? AttachmentFileName,
    string? AttachmentPath,
    IReadOnlyCollection<ExamForStudentOptionDto> Options);

public sealed record ExamForStudentOptionDto(
    Guid OptionId,
    string Text,
    int Order);