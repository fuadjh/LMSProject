using Domain.Common;

namespace Domain.Entities.Users;

public sealed class StudentProfile : BaseEntity
{
    private StudentProfile()
    {
    }

    public Guid UserProfileId { get; private set; }

    public string StudentNumber { get; private set; } = string.Empty;

    public Guid MajorId { get; private set; }

    public static StudentProfile Create(
        Guid userProfileId,
        string studentNumber,
        Guid majorId)
    {
        var profile = new StudentProfile
        {
            Id = Guid.NewGuid(),
            UserProfileId = ValidateUserProfileId(userProfileId)
        };

        profile.Update(studentNumber, majorId);

        return profile;
    }

    public void Update(string studentNumber, Guid majorId)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
            throw new ArgumentException(
                "شماره دانشجویی الزامی است.",
                nameof(studentNumber));

        if (majorId == Guid.Empty)
            throw new ArgumentException(
                "رشته تحصیلی الزامی است.",
                nameof(majorId));

        StudentNumber = studentNumber.Trim();
        MajorId = majorId;
    }

    private static Guid ValidateUserProfileId(Guid userProfileId)
    {
        if (userProfileId == Guid.Empty)
            throw new ArgumentException(
                "شناسه پروفایل کاربر الزامی است.",
                nameof(userProfileId));

        return userProfileId;
    }
}