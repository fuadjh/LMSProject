using Domain.Common;

namespace Domain.Entities.Academics;

public sealed class Semester : BaseEntity
{
    private Semester() { }

    public string Title { get; private set; } = default!;
    public DateTime StartsAtUtc { get; private set; }
    public DateTime EndsAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    public static Semester Create(string title, DateTime startsAtUtc, DateTime endsAtUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.");

        if (endsAtUtc <= startsAtUtc)
            throw new ArgumentException("End date must be after start date.");

        return new Semester
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            StartsAtUtc = startsAtUtc,
            EndsAtUtc = endsAtUtc,
            IsActive = true
        };
    }

    public void Update(string title, DateTime startsAtUtc, DateTime endsAtUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.");

        if (endsAtUtc <= startsAtUtc)
            throw new ArgumentException("End date must be after start date.");

        Title = title.Trim();
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}