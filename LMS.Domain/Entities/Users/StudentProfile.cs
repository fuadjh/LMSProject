using Domain.Common;

namespace Domain.Entities.Users;

public sealed class StudentProfile : BaseEntity
{
    private StudentProfile()
    {
    }

    public string StudentNumber { get; private set; } = null!;

    public Guid MajorId { get; private set; }

    public bool IsActive { get; private set; }

    public static StudentProfile Create(
        Guid userId,
        string studentNumber,
        Guid majorId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "شناسه کاربر الزامی است.",
                nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            throw new ArgumentException(
                "شماره دانشجویی الزامی است.",
                nameof(studentNumber));
        }

        if (majorId == Guid.Empty)
        {
            throw new ArgumentException(
                "رشته تحصیلی الزامی است.",
                nameof(majorId));
        }

        return new StudentProfile
        {
            Id = userId,
            StudentNumber = studentNumber.Trim(),
            MajorId = majorId,
            IsActive = true
        };
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    // عمداً متد UpdateMajor وجود ندارد.
    // رشته دانشجو پس از ایجاد از این چرخه قابل ویرایش نیست.
}