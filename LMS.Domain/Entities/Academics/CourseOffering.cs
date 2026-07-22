using Domain.Common;

namespace Domain.Entities.Academics;

public sealed class CourseOffering : BaseEntity
{
    private CourseOffering() { }

    public Guid CourseId { get; private set; }
    public Guid SemesterId { get; private set; }
    public Guid? InstructorProfileId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? InstructorAssignedAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    public static CourseOffering Create(Guid courseId, Guid semesterId)
    {
        if (courseId == Guid.Empty)
            throw new ArgumentException("CourseId is required.");

        if (semesterId == Guid.Empty)
            throw new ArgumentException("SemesterId is required.");

        return new CourseOffering
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            SemesterId = semesterId,
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };
    }

    public void AssignInstructor(Guid instructorProfileId)
    {
        if (instructorProfileId == Guid.Empty)
            throw new ArgumentException("InstructorProfileId is required.");

        InstructorProfileId = instructorProfileId;
        InstructorAssignedAtUtc = DateTime.UtcNow;
    }

    public void UnassignInstructor()
    {
        InstructorProfileId = null;
        InstructorAssignedAtUtc = null;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}