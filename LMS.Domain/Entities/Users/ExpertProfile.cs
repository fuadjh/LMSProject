using Domain.Common;

namespace Domain.Entities.Users;

public sealed class ExpertProfile : BaseEntity
{
    private ExpertProfile()
    {
    }

    public Guid UserProfileId { get; private set; }

    public static ExpertProfile Create(Guid userProfileId)
    {
        if (userProfileId == Guid.Empty)
            throw new ArgumentException(
                "شناسه پروفایل کاربر الزامی است.",
                nameof(userProfileId));

        return new ExpertProfile
        {
            Id = Guid.NewGuid(),
            UserProfileId = userProfileId
        };
    }
}