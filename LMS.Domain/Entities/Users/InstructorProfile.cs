using Domain.Common;

namespace Domain.Entities.Users;

public sealed class InstructorProfile : BaseEntity
{
    private InstructorProfile()
    {
    }

    public string PersonnelCode { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public static InstructorProfile Create(
        Guid userId,
        string personnelCode)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "شناسه کاربر الزامی است.",
                nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(personnelCode))
        {
            throw new ArgumentException(
                "کد پرسنلی استاد الزامی است.",
                nameof(personnelCode));
        }

        return new InstructorProfile
        {
            Id = userId,
            PersonnelCode = personnelCode.Trim(),
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
}