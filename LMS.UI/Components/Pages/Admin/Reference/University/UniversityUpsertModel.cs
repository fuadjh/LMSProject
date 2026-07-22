namespace WebUi.Pages.Admin.Reference.University;

public sealed class UniversityUpsertModel
{
    public Guid? Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}