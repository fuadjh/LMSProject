using Domain.Common;

namespace Domain.Entities.Academics;

public sealed class Enrollment : BaseEntity
{
    private Enrollment() { }

    public Guid CourseOfferingId { get; private set; }
    public Guid StudentProfileId { get; private set; }
    public DateTime EnrolledAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    public static Enrollment Create(Guid offeringId, Guid studentId)
    {
        if (offeringId == Guid.Empty) throw new ArgumentException("CourseOfferingId is required.");
        if (studentId == Guid.Empty) throw new ArgumentException("StudentProfileId is required.");

        return new Enrollment
        {
            Id = Guid.NewGuid(),
            CourseOfferingId = offeringId,
            StudentProfileId = studentId,
            EnrolledAtUtc = DateTime.UtcNow,
            IsActive = true
        };
    }

    public void Activate()
    {
        IsActive = true;
        EnrolledAtUtc = DateTime.UtcNow;
    }

    public void Deactivate() => IsActive = false;
}
