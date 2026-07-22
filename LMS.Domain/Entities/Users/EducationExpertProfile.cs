using Domain.Common;

namespace Domain.Entities.Users;

public sealed class EducationExpertProfile : BaseEntity
{
    private EducationExpertProfile() { }

    public Guid UserProfileId { get; private set; }
    public string EmployeeCode { get; private set; } = default!;

    public static EducationExpertProfile Create(Guid userProfileId, string employeeCode)
    {
        if (userProfileId == Guid.Empty)
            throw new ArgumentException("UserProfileId is required.");

        if (string.IsNullOrWhiteSpace(employeeCode))
            throw new ArgumentException("EmployeeCode is required.");

        return new EducationExpertProfile
        {
            Id = Guid.NewGuid(),
            UserProfileId = userProfileId,
            EmployeeCode = employeeCode.Trim()
        };
    }
}