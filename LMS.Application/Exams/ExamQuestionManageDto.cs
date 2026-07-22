using Common.Enums;


namespace LMS.Application.Exams;

public sealed record ExamQuestionManageDto(
    Guid QuestionId,
    string Title,
    QuestionType Type,
    decimal Score,
    int Order);