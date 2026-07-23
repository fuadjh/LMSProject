using Common.Enums;

namespace WebUi.Pages.Admin.Academic;

public sealed class CourseVm
{
    public Guid? Id { get; set; }
    public Guid? MajorId { get; set; }
    public string MajorTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Units { get; set; } = 3;
    public CourseEnrollmentScope EnrollmentScope { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class SemesterVm
{
    public Guid? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? StartsAtUtc { get; set; }
    public DateTime? EndsAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class OfferingVm
{
    public Guid? Id { get; set; }
    public Guid? CourseId { get; set; }
    public Guid? SemesterId { get; set; }
    public Guid MajorId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string SemesterTitle { get; set; } = string.Empty;
    public string SectionCode { get; set; } = string.Empty;
    public int Capacity { get; set; } = 30;
    public int ActiveEnrollmentCount { get; set; }
    public CourseEnrollmentScope EnrollmentScope { get; set; }
    public Guid? InstructorProfileId { get; set; }
    public string? InstructorName { get; set; }
    public bool IsActive { get; set; } = true;
}
