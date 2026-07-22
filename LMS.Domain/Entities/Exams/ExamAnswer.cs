using Domain.Common;

namespace Domain.Entities.Exams;

public sealed class ExamAnswer : BaseEntity
{
    private ExamAnswer() { }

    public Guid ExamSubmissionId { get; private set; }
    public Guid QuestionId { get; private set; }
    public Guid? SelectedOptionId { get; private set; }
    public string? EssayText { get; private set; }
    public string? EssayAttachmentFileName { get; private set; }
    public string? EssayAttachmentPath { get; private set; }
    public decimal? AwardedScore { get; private set; }

    public static ExamAnswer Create(
        Guid examSubmissionId,
        Guid questionId,
        Guid? selectedOptionId,
        string? essayText,
        string? essayAttachmentFileName,
        string? essayAttachmentPath,
        decimal? awardedScore)
    {
        if (examSubmissionId == Guid.Empty)
            throw new ArgumentException("ExamSubmissionId is required.");

        if (questionId == Guid.Empty)
            throw new ArgumentException("QuestionId is required.");

        return new ExamAnswer
        {
            Id = Guid.NewGuid(),
            ExamSubmissionId = examSubmissionId,
            QuestionId = questionId,
            SelectedOptionId = selectedOptionId,
            EssayText = essayText,
            EssayAttachmentFileName = essayAttachmentFileName,
            EssayAttachmentPath = essayAttachmentPath,
            AwardedScore = awardedScore
        };
    }

    public void Grade(decimal score)
    {
        if (score < 0)
            throw new ArgumentException("Score cannot be negative.");

        AwardedScore = score;
    }
}