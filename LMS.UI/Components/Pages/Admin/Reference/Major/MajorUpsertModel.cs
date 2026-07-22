namespace WebUi.Pages.Admin.Reference.Major;

public sealed class MajorUpsertModel
{
    public Guid? Id { get; set; }

    public Guid UniversityId { get; set; }

    public string UniversityTitle { get; set; } = string.Empty;

    public Guid FacultyId { get; set; }

    public string FacultyTitle { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}