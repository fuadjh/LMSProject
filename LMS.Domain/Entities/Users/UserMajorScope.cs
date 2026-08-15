using Common.Enums;
using Domain.Common;

namespace Domain.Entities.Users;

public sealed class UserMajorScope : BaseEntity
{
    private UserMajorScope()
    {
    }

    public Guid UserId { get; private set; }

    public UserRoleType RoleType { get; private set; }

    public Guid MajorId { get; private set; }

    public static UserMajorScope Create(
        Guid userId,
        UserRoleType roleType,
        Guid majorId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "شناسه کاربر الزامی است.",
                nameof(userId));
        }

        if (!Enum.IsDefined(roleType))
        {
            throw new ArgumentOutOfRangeException(
                nameof(roleType),
                roleType,
                "نوع نقش معتبر نیست.");
        }

        if (majorId == Guid.Empty)
        {
            throw new ArgumentException(
                "شناسه رشته الزامی است.",
                nameof(majorId));
        }

        return new UserMajorScope
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleType = roleType,
            MajorId = majorId
        };
    }
}