using Domain.Common;

namespace Domain.Entities.Users;

public sealed class UserFacultyScope : BaseEntity
{
    private UserFacultyScope() { }

    public Guid UserProfileId { get; private set; }
    public Guid FacultyId { get; private set; }

    public static UserFacultyScope Create(Guid userProfileId, Guid facultyId)
    {
        if (userProfileId == Guid.Empty)
            throw new ArgumentException("UserProfileId is required.");

        if (facultyId == Guid.Empty)
            throw new ArgumentException("FacultyId is required.");

        return new UserFacultyScope
        {
            Id = Guid.NewGuid(),
            UserProfileId = userProfileId,
            FacultyId = facultyId
        };
    }
}