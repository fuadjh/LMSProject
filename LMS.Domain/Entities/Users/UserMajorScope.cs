using Domain.Common;

namespace Domain.Entities.Users;

public sealed class UserMajorScope : BaseEntity
{
    private UserMajorScope() { }

    public Guid UserProfileId { get; private set; }
    public Guid MajorId { get; private set; }

    public static UserMajorScope Create(Guid userProfileId, Guid majorId)
    {
        if (userProfileId == Guid.Empty)
            throw new ArgumentException("UserProfileId is required.");

        if (majorId == Guid.Empty)
            throw new ArgumentException("MajorId is required.");

        return new UserMajorScope
        {
            Id = Guid.NewGuid(),
            UserProfileId = userProfileId,
            MajorId = majorId
        };
    }
}