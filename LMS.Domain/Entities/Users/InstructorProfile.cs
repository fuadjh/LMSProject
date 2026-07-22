using Domain.Common;

namespace Domain.Entities.Users;

public sealed class InstructorProfile : BaseEntity
{
    private InstructorProfile() { }

    public Guid UserProfileId { get; private set; }
    public string PersonnelCode { get; private set; } = default!;

    public static InstructorProfile Create(Guid userProfileId, string personnelCode)
    {
        if (userProfileId == Guid.Empty)
            throw new ArgumentException("UserProfileId is required.");

        if (string.IsNullOrWhiteSpace(personnelCode))
            throw new ArgumentException("PersonnelCode is required.");

        return new InstructorProfile
        {
            Id = Guid.NewGuid(),
            UserProfileId = userProfileId,
            PersonnelCode = personnelCode.Trim()
        };
    }
}