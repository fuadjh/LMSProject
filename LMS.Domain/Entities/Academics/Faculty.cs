using Domain.Common;

namespace Domain.Entities.Academics;

public sealed class Faculty : BaseEntity
{
    private Faculty() { }

    public Guid UniversityId { get; private set; }

    public string Title { get; private set; } = default!;

    public string Code { get; private set; } = default!;

    public bool IsActive { get; private set; }

    public static Faculty Create(
        Guid universityId,
        string title,
        string code)
    {
        ValidateUniversityId(universityId);
        ValidateTitle(title);
        ValidateCode(code);

        return new Faculty
        {
            Id = Guid.NewGuid(),
            UniversityId = universityId,
            Title = title.Trim(),
            Code = code.Trim(),
            IsActive = true
        };
    }

    public void Update(
        Guid universityId,
        string title,
        string code)
    {
        ValidateUniversityId(universityId);
        ValidateTitle(title);
        ValidateCode(code);

        UniversityId = universityId;
        Title = title.Trim();
        Code = code.Trim();
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private static void ValidateUniversityId(Guid universityId)
    {
        if (universityId == Guid.Empty)
            throw new ArgumentException("UniversityId is required.");
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.");
    }

    private static void ValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required.");
    }
}