using Common.Enums;
using Domain.Common;


namespace Domain.Entities.Exams;

public sealed class ExamSubmission : BaseEntity
{
    private ExamSubmission() { }

    public Guid ExamId { get; private set; }
    public Guid StudentProfileId { get; private set; }
    public int AttemptNumber { get; private set; }
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? SubmittedAtUtc { get; private set; }
    public ExamSubmissionStatus Status { get; private set; }
    public decimal? TotalScore { get; private set; }

    public static ExamSubmission Create(Guid examId, Guid studentProfileId, int attemptNumber)
    {
        if (examId == Guid.Empty)
            throw new ArgumentException("ExamId is required.");

        if (studentProfileId == Guid.Empty)
            throw new ArgumentException("StudentProfileId is required.");

        if (attemptNumber <= 0)
            throw new ArgumentException("AttemptNumber must be greater than zero.");

        return new ExamSubmission
        {
            Id = Guid.NewGuid(),
            ExamId = examId,
            StudentProfileId = studentProfileId,
            AttemptNumber = attemptNumber,
            StartedAtUtc = DateTime.UtcNow,
            Status = ExamSubmissionStatus.InProgress
        };
    }

    public void Submit(decimal initialScore)
    {
        SubmittedAtUtc = DateTime.UtcNow;
        Status = ExamSubmissionStatus.Submitted;
        TotalScore = initialScore;
    }

    public void MarkGraded(decimal totalScore)
    {
        Status = ExamSubmissionStatus.Graded;
        TotalScore = totalScore;
    }
}