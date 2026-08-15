using Common.Enums;
using Domain.Common;

namespace Domain.Entities.Users;

public sealed class UserFacultyScope : BaseEntity
{
    private UserFacultyScope()
    {
    }

    public Guid UserId { get; private set; }

    public UserRoleType RoleType { get; private set; }

    public Guid FacultyId { get; private set; }

    public static UserFacultyScope Create(
        Guid userId,
        UserRoleType roleType,
        Guid facultyId)
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

        if (facultyId == Guid.Empty)
        {
            throw new ArgumentException(
                "شناسه دانشکده الزامی است.",
                nameof(facultyId));
        }

        return new UserFacultyScope
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleType = roleType,
            FacultyId = facultyId
        };
    }
}