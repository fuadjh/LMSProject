using Domain.Common;

namespace Domain.Entities.Users;

public sealed class InstructorProfile : BaseEntity
{
    private InstructorProfile()
    {
    }

    public Guid UserProfileId { get; private set; }

    public string PersonnelCode { get; private set; } = string.Empty;

    public static InstructorProfile Create(
        Guid userProfileId,
        string personnelCode)
    {
        if (userProfileId == Guid.Empty)
            throw new ArgumentException(
                "شناسه پروفایل کاربر الزامی است.",
                nameof(userProfileId));

        var profile = new InstructorProfile
        {
            Id = Guid.NewGuid(),
            UserProfileId = userProfileId
        };

        profile.Update(personnelCode);

        return profile;
    }

    public void Update(string personnelCode)
    {
        if (string.IsNullOrWhiteSpace(personnelCode))
            throw new ArgumentException(
                "کد پرسنلی الزامی است.",
                nameof(personnelCode));

        PersonnelCode = personnelCode.Trim();
    }
}