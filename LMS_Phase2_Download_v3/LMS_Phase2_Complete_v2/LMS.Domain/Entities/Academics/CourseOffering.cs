using Domain.Common;

namespace Domain.Entities.Academics;

public sealed class CourseOffering : BaseEntity
{
    private CourseOffering() { }

    public Guid CourseId { get; private set; }
    public Guid SemesterId { get; private set; }
    public string SectionCode { get; private set; } = default!;
    public int Capacity { get; private set; }
    public Guid? InstructorProfileId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? InstructorAssignedAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    public static CourseOffering Create(
        Guid courseId, Guid semesterId, string sectionCode, int capacity)
    {
        Validate(courseId, semesterId, sectionCode, capacity);
        return new CourseOffering
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            SemesterId = semesterId,
            SectionCode = sectionCode.Trim().ToUpperInvariant(),
            Capacity = capacity,
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };
    }

    public void Update(string sectionCode, int capacity, int activeCount)
    {
        Validate(CourseId, SemesterId, sectionCode, capacity);
        if (capacity < activeCount)
            throw new InvalidOperationException(
                "ظرفیت نمی‌تواند کمتر از تعداد ثبت‌نام فعال باشد.");

        SectionCode = sectionCode.Trim().ToUpperInvariant();
        Capacity = capacity;
    }

    public bool HasAvailableCapacity(int activeCount) =>
        IsActive && activeCount < Capacity;

    public void AssignInstructor(Guid instructorId)
    {
        if (instructorId == Guid.Empty)
            throw new ArgumentException("InstructorProfileId is required.");

        InstructorProfileId = instructorId;
        InstructorAssignedAtUtc = DateTime.UtcNow;
    }

    public void UnassignInstructor()
    {
        InstructorProfileId = null;
        InstructorAssignedAtUtc = null;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    private static void Validate(
        Guid courseId, Guid semesterId, string sectionCode, int capacity)
    {
        if (courseId == Guid.Empty) throw new ArgumentException("CourseId is required.");
        if (semesterId == Guid.Empty) throw new ArgumentException("SemesterId is required.");
        if (string.IsNullOrWhiteSpace(sectionCode) || sectionCode.Trim().Length > 20)
            throw new ArgumentException("SectionCode is invalid.");
        if (capacity is < 1 or > 500)
            throw new ArgumentException("Capacity must be between 1 and 500.");
    }
}
