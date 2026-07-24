using Domain.Common;

namespace Domain.Entities.Academics;

public sealed class CourseOffering : BaseEntity
{
    private CourseOffering()
    {
    }

    public Guid CourseId { get; private set; }

    public Guid SemesterId { get; private set; }

    public string SectionCode { get; private set; } = default!;

    public int Capacity { get; private set; }

    public DateTime StartsAtUtc { get; private set; }

    public DateTime EndsAtUtc { get; private set; }

    public Guid? InstructorProfileId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? InstructorAssignedAtUtc { get; private set; }

    public bool IsActive { get; private set; }

    public static CourseOffering Create(
        Guid courseId,
        Guid semesterId,
        string sectionCode,
        int capacity,
        DateTime startsAtUtc,
        DateTime endsAtUtc)
    {
        Validate(
            courseId,
            semesterId,
            sectionCode,
            capacity,
            startsAtUtc,
            endsAtUtc);

        return new CourseOffering
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            SemesterId = semesterId,
            SectionCode = NormalizeSectionCode(sectionCode),
            Capacity = capacity,
            StartsAtUtc = startsAtUtc,
            EndsAtUtc = endsAtUtc,
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };
    }

    public void Update(
        string sectionCode,
        int capacity,
        DateTime startsAtUtc,
        DateTime endsAtUtc)
    {
        Validate(
            CourseId,
            SemesterId,
            sectionCode,
            capacity,
            startsAtUtc,
            endsAtUtc);

        SectionCode = NormalizeSectionCode(sectionCode);
        Capacity = capacity;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
    }

    public bool HasCapacity(int activeEnrollmentCount) =>
        activeEnrollmentCount < Capacity;

    public void AssignInstructor(Guid instructorProfileId)
    {
        if (instructorProfileId == Guid.Empty)
            throw new ArgumentException(
                "InstructorProfileId is required.");

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

    private static void Validate(
        Guid courseId,
        Guid semesterId,
        string sectionCode,
        int capacity,
        DateTime startsAtUtc,
        DateTime endsAtUtc)
    {
        if (courseId == Guid.Empty)
            throw new ArgumentException("CourseId is required.");

        if (semesterId == Guid.Empty)
            throw new ArgumentException("SemesterId is required.");

        if (string.IsNullOrWhiteSpace(sectionCode))
            throw new ArgumentException("SectionCode is required.");

        if (sectionCode.Trim().Length > 20)
            throw new ArgumentException(
                "SectionCode cannot exceed 20 characters.");

        if (capacity is < 1 or > 500)
            throw new ArgumentException(
                "Capacity must be between 1 and 500.");

        if (endsAtUtc <= startsAtUtc)
            throw new ArgumentException(
                "End date must be after start date.");
    }

    private static string NormalizeSectionCode(string sectionCode) =>
        sectionCode.Trim().ToUpperInvariant();
}