using Common.Enums;

namespace Application.Common.Models;

public sealed record UserLookupDto(
    Guid UserId,
    string UserName,
    string FullName,
    string NationalCode,
    string? PhoneNumber);

public sealed record UserListItemDto(
    Guid UserId,
    UserRoleType RoleType,
    string UserName,
    string FirstName,
    string LastName,
    string FullName,
    string NationalCode,
    string? PhoneNumber,
    string? Email,
    bool IsActive,
    bool RoleIsActive,
    string? RoleIdentifier,
    Guid? FacultyId,
    Guid? MajorId,
    string? MajorTitle);

public sealed record UserDetailsDto(
    Guid UserId,
    UserRoleType RoleType,
    string UserName,
    string FirstName,
    string LastName,
    string NationalCode,
    string? PhoneNumber,
    string? Email,
    string? LatinFirstName,
    string? LatinLastName,
    Gender Gender,
    string? ProfileImagePath,
    bool IsActive,
    bool RoleIsActive,
    string? RoleIdentifier,
    Guid? FacultyId,
    Guid? MajorId,
    string? MajorTitle,
    IReadOnlyCollection<Guid> FacultyScopeIds,
    IReadOnlyCollection<Guid> MajorScopeIds,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);

public sealed record UserAccessDetailsDto(
    Guid UserId,
    string UserName,
    string FullName,
    bool IsActive,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions,
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