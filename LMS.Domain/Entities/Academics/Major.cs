using Domain.Common;

namespace Domain.Entities.Academics;

public sealed class Major : BaseEntity
{
    private Major() { }

    public Guid FacultyId { get; private set; }
    public string Title { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public bool IsActive { get; private set; }

    public static Major Create(Guid facultyId, string title, string code)
    {
        if (facultyId == Guid.Empty)
            throw new ArgumentException("FacultyId is required.");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.");

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required.");

        return new Major
        {
            Id = Guid.NewGuid(),
            FacultyId = facultyId,
            Title = title.Trim(),
            Code = code.Trim(),
            IsActive = true
        };
    }

    public void Update(string title, string code)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.");

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required.");

        Title = title.Trim();
        Code = code.Trim();
    }

    public void MoveToFaculty(Guid facultyId)
    {
        if (facultyId == Guid.Empty)
            throw new ArgumentException("FacultyId is required.");

        FacultyId = facultyId;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}