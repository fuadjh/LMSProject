using Common.Enums;

namespace Common.Contracts.Exams;

public sealed record QuestionBankItemDto(
    Guid QuestionId,
    string Title,
    string Body,
    QuestionType Type,
    QuestionDifficulty Difficulty,
    EvaluationDomain EvaluationDomain,
    QuestionSourceType SourceType,
    string? SourceDescription,
    decimal SuggestedScore,
    string? AttachmentFileName,
    string? AttachmentPath,
    IReadOnlyCollection<QuestionOptionDto> Options);