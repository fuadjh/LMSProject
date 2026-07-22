using Domain.Common;

namespace Domain.Entities.Users;

public sealed class StudentProfile : BaseEntity
{
    private StudentProfile() { }

    public Guid UserProfileId { get; private set; }
    public string StudentNumber { get; private set; } = default!;
    public Guid MajorId { get; private set; }

    public static StudentProfile Create(Guid userProfileId, string studentNumber, Guid majorId)
    {
        if (userProfileId == Guid.Empty)
            throw new ArgumentException("UserProfileId is required.");

        if (string.IsNullOrWhiteSpace(studentNumber))
            throw new ArgumentException("StudentNumber is required.");

        if (majorId == Guid.Empty)
            throw new ArgumentException("MajorId is required.");

        return new StudentProfile
        {
            Id = Guid.NewGuid(),
            UserProfileId = userProfileId,
            StudentNumber = studentNumber.Trim(),
            MajorId = majorId
        };
    }
}