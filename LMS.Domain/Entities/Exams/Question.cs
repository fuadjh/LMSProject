using Common.Enums;
using Domain.Common;


namespace Domain.Entities.Exams;

public sealed class Question : BaseEntity
{
    private Question() { }

    public Guid QuestionBankId { get; private set; }
    public string Title { get; private set; } = default!;
    public string Body { get; private set; } = default!;
    public QuestionType Type { get; private set; }
    public QuestionDifficulty Difficulty { get; private set; }
    public EvaluationDomain EvaluationDomain { get; private set; }
    public QuestionSourceType SourceType { get; private set; }
    public string? SourceDescription { get; private set; }
    public decimal SuggestedScore { get; private set; }
    public string? AttachmentFileName { get; private set; }
    public string? AttachmentPath { get; private set; }
    public bool IsActive { get; private set; }

    public static Question Create(
        Guid questionBankId,
        string title,
        string body,
        QuestionType type,
        QuestionDifficulty difficulty,
        EvaluationDomain evaluationDomain,
        QuestionSourceType sourceType,
        string? sourceDescription,
        decimal suggestedScore,
        string? attachmentFileName,
        string? attachmentPath)
    {
        if (questionBankId == Guid.Empty)
            throw new ArgumentException("QuestionBankId is required.");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.");

        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body is required.");

        if (suggestedScore <= 0)
            throw new ArgumentException("SuggestedScore must be greater than zero.");

        return new Question
        {
            Id = Guid.NewGuid(),
            QuestionBankId = questionBankId,
            Title = title.Trim(),
            Body = body.Trim(),
            Type = type,
            Difficulty = difficulty,
            EvaluationDomain = evaluationDomain,
            SourceType = sourceType,
            SourceDescription = sourceDescription?.Trim(),
            SuggestedScore = suggestedScore,
            AttachmentFileName = attachmentFileName,
            AttachmentPath = attachmentPath,
            IsActive = true
        };
    }
}