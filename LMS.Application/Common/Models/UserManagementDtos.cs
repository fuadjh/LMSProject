namespace Application.Common.Models;

public enum UserProfileType
{
    Student = 1,
    Instructor = 2,
    EducationExpert = 3
}

public sealed record UserProfileListItemDto(
    Guid UserProfileId,
    Guid AuthUserId,
    string? NationalCode,
    string UserName,
    string Email,
    string FullName,
    string ProfileCode,
    string? MajorTitle,
    bool IsActive);

public sealed record UserProfileDetailsDto(
    Guid UserProfileId,
    Guid AuthUserId,
    string? NationalCode,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    UserProfileType ProfileType,
    string ProfileCode,
    Guid? MajorId,
    Guid? FacultyId,
    string? MajorTitle,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<Guid> FacultyIds,
    IReadOnlyCollection<Guid> MajorIds);

public sealed record UserByNationalCodeDto(
    Guid UserProfileId,
    Guid AuthUserId,
    string NationalCode,
    string FullName,
    string UserName,
    string Email,
    bool IsActive,
    bool HasStudentProfile,
    bool HasInstructorProfile,
    bool HasEducationExpertProfile);