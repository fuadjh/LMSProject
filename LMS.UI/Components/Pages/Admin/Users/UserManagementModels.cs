using System.ComponentModel.DataAnnotations;

namespace WebUi.Components.Pages.Admin.Users;

public enum UserProfileTypeVm
{
    Student = 1,
    Instructor = 2,
    EducationExpert = 3
}

public sealed class UserProfileListItemVm
{
    public Guid UserProfileId { get; set; }
    public Guid AuthUserId { get; set; }
    public string? NationalCode { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string ProfileCode { get; set; } = string.Empty;
    public string? MajorTitle { get; set; }
    public bool IsActive { get; set; }
}

public sealed class UserProfileDetailsVm
{
    public Guid UserProfileId { get; set; }
    public Guid AuthUserId { get; set; }
    public string? NationalCode { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public UserProfileTypeVm ProfileType { get; set; }
    public string ProfileCode { get; set; } = string.Empty;
    public Guid? MajorId { get; set; }
    public Guid? FacultyId { get; set; }
    public string? MajorTitle { get; set; }

    public IReadOnlyCollection<string> Roles { get; set; }
        = Array.Empty<string>();

    public IReadOnlyCollection<Guid> FacultyIds { get; set; }
        = Array.Empty<Guid>();

    public IReadOnlyCollection<Guid> MajorIds { get; set; }
        = Array.Empty<Guid>();
}

public sealed class UserEditModel
{
    public Guid UserProfileId { get; set; }

    [Required(ErrorMessage = "نام الزامی است.")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام خانوادگی الزامی است.")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "ایمیل الزامی است.")]
    [EmailAddress(ErrorMessage = "فرمت ایمیل معتبر نیست.")]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;
}

public sealed class ExistingPersonVm
{
    public Guid UserProfileId { get; set; }
    public Guid AuthUserId { get; set; }
    public string NationalCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool HasStudentProfile { get; set; }
    public bool HasInstructorProfile { get; set; }
    public bool HasEducationExpertProfile { get; set; }
}

public sealed class UserAccessDetailsVm
{
    public Guid UserProfileId { get; set; }
    public Guid AuthUserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    public IReadOnlyCollection<string> Roles { get; set; }
        = Array.Empty<string>();

    public IReadOnlyCollection<Guid> FacultyIds { get; set; }
        = Array.Empty<Guid>();

    public IReadOnlyCollection<Guid> MajorIds { get; set; }
        = Array.Empty<Guid>();
}

public sealed class RoleVm
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class LookupVm
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
}

public sealed class PagedResponseVm<T>
{
    public IReadOnlyCollection<T> Items { get; set; }
        = Array.Empty<T>();

    public int TotalCount { get; set; }
}

public abstract class CreateUserBaseVm
{
    [Required(ErrorMessage = "کد ملی الزامی است.")]
    public string NationalCode { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public sealed class CreateStudentVm : CreateUserBaseVm
{
    [Required(ErrorMessage = "شماره دانشجویی الزامی است.")]
    public string StudentNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "انتخاب رشته الزامی است.")]
    public Guid? MajorId { get; set; }
}

public sealed class CreateInstructorVm : CreateUserBaseVm
{
    [Required(ErrorMessage = "کد پرسنلی الزامی است.")]
    public string PersonnelCode { get; set; } = string.Empty;
}

public sealed class CreateEducationExpertVm : CreateUserBaseVm
{
    [Required(ErrorMessage = "کد کارمندی الزامی است.")]
    public string EmployeeCode { get; set; } = string.Empty;
}