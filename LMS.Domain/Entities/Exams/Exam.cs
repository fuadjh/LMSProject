using Common.Enums;
using Domain.Common;


namespace Domain.Entities.Exams;

public sealed class Exam : BaseEntity
{
    private Exam() { }

    public Guid CourseOfferingId { get; private set; }
    public Guid CreatedByInstructorProfileId { get; private set; }
    public string Title { get; private set; } = default!;
    public ExamStatus Status { get; private set; }
    public ExamDeliveryMode DeliveryMode { get; private set; }
    public DateTime StartsAtUtc { get; private set; }
    public DateTime EndsAtUtc { get; private set; }
    public int DurationMinutes { get; private set; }
    public int MaxAttemptsPerStudent { get; private set; }
    public bool IsActive { get; private set; }

    public static Exam Create(
        Guid courseOfferingId,
        Guid createdByInstructorProfileId,
        string title,
        ExamDeliveryMode deliveryMode,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        int durationMinutes,
        int maxAttemptsPerStudent)
    {
        if (courseOfferingId == Guid.Empty)
            throw new ArgumentException("CourseOfferingId is required.");

        if (createdByInstructorProfileId == Guid.Empty)
            throw new ArgumentException("CreatedByInstructorProfileId is required.");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.");

        if (endsAtUtc <= startsAtUtc)
            throw new ArgumentException("End date must be after start date.");

        if (durationMinutes <= 0)
            throw new ArgumentException("DurationMinutes must be greater than zero.");

        if (maxAttemptsPerStudent <= 0)
            throw new ArgumentException("MaxAttemptsPerStudent must be greater than zero.");

        return new Exam
        {
            Id = Guid.NewGuid(),
            CourseOfferingId = courseOfferingId,
            CreatedByInstructorProfileId = createdByInstructorProfileId,
            Title = title.Trim(),
            Status = ExamStatus.Draft,
            DeliveryMode = deliveryMode,
            StartsAtUtc = startsAtUtc,
            EndsAtUtc = endsAtUtc,
            DurationMinutes = durationMinutes,
            MaxAttemptsPerStudent = maxAttemptsPerStudent,
            IsActive = true
        };
    }

    public void Publish() => Status = ExamStatus.Published;
    public void Close() => Status = ExamStatus.Closed;
}