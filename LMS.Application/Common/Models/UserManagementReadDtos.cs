using Common.Enums;
using Common.Security;

namespace Application.Common.Models;

public sealed record UserProfileListItemDto(
    Guid UserId,
    Guid UserProfileId,
    string UserName,
    string FullName,
    string NationalCode,
    string PhoneNumber,
    UserRoleType Role,
    bool IsActive,
    string? StudentNumber,
    string? PersonnelCode);

public sealed record UserProfileDetailsDto(
    Guid UserId,
    Guid UserProfileId,
    string UserName,
    string FirstName,
    string LastName,
    string NationalCode,
    string PhoneNumber,
    string? Email,
    string? LatinFirstName,
    string? LatinLastName,
    Gender Gender,
    string? ProfileImagePath,
    bool IsActive,
    UserRoleType Role,
    string? StudentNumber,
    Guid? StudentMajorId,
    string? PersonnelCode,
    IReadOnlyCollection<Guid> FacultyScopeIds,
    IReadOnlyCollection<Guid> MajorScopeIds);

public sealed record UserByNationalCodeDto(
    Guid UserId,
    string UserName,
    string FirstName,
    string LastName,
    string NationalCode,
    string? PhoneNumber,
    string? Email,
    bool IsActive,
    bool HasStudentProfile,
    bool HasInstructorProfile,
    bool HasExpertProfile);

public sealed record UserAccessDetailsDto(
    Guid UserId,
    string FullName,
    string UserName,
    bool IsActive,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions,
    IReadOnlyCollection<Guid> FacultyScopeIds,
    IReadOnlyCollection<Guid> MajorScopeIds);

public sealed record UserLookupDto(
    Guid UserProfileId,
    Guid UserId,
    string FullName,
    string UserName,
    bool IsActive);