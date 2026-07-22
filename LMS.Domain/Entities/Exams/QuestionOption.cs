using Domain.Common;

namespace Domain.Entities.Exams;

public sealed class QuestionOption : BaseEntity
{
    private QuestionOption() { }

    public Guid QuestionId { get; private set; }
    public string Text { get; private set; } = default!;
    public int Order { get; private set; }
    public bool IsCorrect { get; private set; }

    public static QuestionOption Create(Guid questionId, string text, int order, bool isCorrect)
    {
        if (questionId == Guid.Empty)
            throw new ArgumentException("QuestionId is required.");

        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text is required.");

        if (order <= 0)
            throw new ArgumentException("Order must be greater than zero.");

        return new QuestionOption
        {
            Id = Guid.NewGuid(),
            QuestionId = questionId,
            Text = text.Trim(),
            Order = order,
            IsCorrect = isCorrect
        };
    }
}