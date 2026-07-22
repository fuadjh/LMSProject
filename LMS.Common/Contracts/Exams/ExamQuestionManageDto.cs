using Common.Enums;


namespace Common.Contracts.Exams;

public sealed record ExamQuestionManageDto(
    Guid QuestionId,
    string Title,
    QuestionType Type,
    decimal Score,
    int Order);