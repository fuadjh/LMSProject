using Domain.Common;

namespace Domain.Entities.Academics;

public sealed class Course : BaseEntity
{
    private Course() { }

    public Guid MajorId { get; private set; }
    public string Title { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public int Units { get; private set; }
    public bool IsActive { get; private set; }

    public static Course Create(Guid majorId, string title, string code, int units)
    {
        if (majorId == Guid.Empty)
            throw new ArgumentException("MajorId is required.");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.");

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required.");

        if (units <= 0)
            throw new ArgumentException("Units must be greater than zero.");

        return new Course
        {
            Id = Guid.NewGuid(),
            MajorId = majorId,
            Title = title.Trim(),
            Code = code.Trim(),
            Units = units,
            IsActive = true
        };
    }

    public void Update(string title, string code, int units)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.");

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required.");

        if (units <= 0)
            throw new ArgumentException("Units must be greater than zero.");

        Title = title.Trim();
        Code = code.Trim();
        Units = units;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}