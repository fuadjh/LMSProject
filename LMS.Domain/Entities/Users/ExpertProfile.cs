using Domain.Common;

namespace Domain.Entities.Users;

public sealed class ExpertProfile : BaseEntity
{
    private ExpertProfile()
    {
    }

    public bool IsActive { get; private set; }

    public static ExpertProfile Create(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "شناسه کاربر الزامی است.",
                nameof(userId));
        }

        return new ExpertProfile
        {
            Id = userId,
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