namespace Common.Contracts.Exams;

public sealed record QuestionOptionDto(
    Guid Id,
    string Text,
    int Order,
    bool IsCorrect);