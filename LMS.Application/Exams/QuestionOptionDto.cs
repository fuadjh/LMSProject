namespace LMS.Application.Exams;

public sealed record QuestionOptionDto(
    Guid Id,
    string Text,
    int Order,
    bool IsCorrect);