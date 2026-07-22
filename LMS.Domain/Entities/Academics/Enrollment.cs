using Domain.Common;

namespace Domain.Entities.Academics;

public sealed class Enrollment : BaseEntity
{
    private Enrollment() { }

    public Guid CourseOfferingId { get; private set; }
    public Guid StudentProfileId { get; private set; }
    public DateTime EnrolledAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    public static Enrollment Create(Guid courseOfferingId, Guid studentProfileId)
    {
        if (courseOfferingId == Guid.Empty)
            throw new ArgumentException("CourseOfferingId is required.");

        if (studentProfileId == Guid.Empty)
            throw new ArgumentException("StudentProfileId is required.");

        return new Enrollment
        {
            Id = Guid.NewGuid(),
            CourseOfferingId = courseOfferingId,
            StudentProfileId = studentProfileId,
            EnrolledAtUtc = DateTime.UtcNow,
            IsActive = true
        };
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}