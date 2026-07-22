using Domain.Common;

namespace Domain.Entities.Academics;

public sealed class University : BaseEntity
{
    private University() { }

    public string Title { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public bool IsActive { get; private set; }

    public static University Create(string title, string code)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.");

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required.");

        return new University
        {
            Id = Guid.NewGuid(),
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

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}