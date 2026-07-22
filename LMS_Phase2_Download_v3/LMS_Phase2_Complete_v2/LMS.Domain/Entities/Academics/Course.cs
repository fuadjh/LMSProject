using Common.Enums;
using Domain.Common;

namespace Domain.Entities.Academics;

public sealed class Course : BaseEntity
{
    private Course() { }

    public Guid MajorId { get; private set; }
    public string Title { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public int Units { get; private set; }
    public CourseEnrollmentScope EnrollmentScope { get; private set; }
    public bool IsActive { get; private set; }

    public static Course Create(
        Guid majorId, string title, string code, int units,
        CourseEnrollmentScope enrollmentScope)
    {
        Validate(majorId, title, code, units, enrollmentScope);
        return new Course
        {
            Id = Guid.NewGuid(),
            MajorId = majorId,
            Title = title.Trim(),
            Code = code.Trim().ToUpperInvariant(),
            Units = units,
            EnrollmentScope = enrollmentScope,
            IsActive = true
        };
    }

    public void Update(
        Guid majorId, string title, string code, int units,
        CourseEnrollmentScope enrollmentScope)
    {
        Validate(majorId, title, code, units, enrollmentScope);
        MajorId = majorId;
        Title = title.Trim();
        Code = code.Trim().ToUpperInvariant();
        Units = units;
        EnrollmentScope = enrollmentScope;
    }

    public bool AllowsEnrollmentFor(Guid studentMajorId) =>
        studentMajorId != Guid.Empty &&
        (EnrollmentScope == CourseEnrollmentScope.AllMajors ||
         MajorId == studentMajorId);

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    private static void Validate(
        Guid majorId, string title, string code, int units,
        CourseEnrollmentScope scope)
    {
        if (majorId == Guid.Empty) throw new ArgumentException("MajorId is required.");
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.");
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.");
        if (units is < 1 or > 30) throw new ArgumentException("Units must be between 1 and 30.");
        if (!Enum.IsDefined(typeof(CourseEnrollmentScope), scope))
            throw new ArgumentException("EnrollmentScope is invalid.");
    }
}
