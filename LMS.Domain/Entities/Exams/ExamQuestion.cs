using Domain.Common;

namespace Domain.Entities.Exams;

public sealed class ExamQuestion : BaseEntity
{
    private ExamQuestion() { }

    public Guid ExamId { get; private set; }
    public Guid QuestionId { get; private set; }
    public int Order { get; private set; }
    public decimal Score { get; private set; }

    public static ExamQuestion Create(Guid examId, Guid questionId, int order, decimal score)
    {
        if (examId == Guid.Empty)
            throw new ArgumentException("ExamId is required.");

        if (questionId == Guid.Empty)
            throw new ArgumentException("QuestionId is required.");

        if (order <= 0)
            throw new ArgumentException("Order must be greater than zero.");

        if (score <= 0)
            throw new ArgumentException("Score must be greater than zero.");

        return new ExamQuestion
        {
            Id = Guid.NewGuid(),
            ExamId = examId,
            QuestionId = questionId,
            Order = order,
            Score = score
        };
    }
}