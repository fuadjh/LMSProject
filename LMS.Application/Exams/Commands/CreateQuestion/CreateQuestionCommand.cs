using Application.Common.Results;
using Common.Enums;

using MediatR;

namespace Application.Exams.Commands.CreateQuestion;

public sealed record CreateQuestionCommand(
    Guid CourseOfferingId,
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
    IReadOnlyCollection<CreateQuestionOptionItem> Options) : IRequest<Result<Guid>>;

public sealed record CreateQuestionOptionItem(
    string Text,
    int Order,
    bool IsCorrect);