namespace WebUi.Components.Pages.Admin.Users;

public enum UserCreateType
{
    Student = 1,
    Instructor = 2,
    Expert = 3
}

public enum GenderVm
{
    Male = 1,
    Female = 2
}

public sealed class CreateUserModel
{
    public UserCreateType Type { get; set; } = UserCreateType.Student;

    public string UserName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string NationalCode { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? LatinFirstName { get; set; }

    public string? LatinLastName { get; set; }

    public GenderVm Gender { get; set; } = GenderVm.Male;

    public string? ProfileImagePath { get; set; }

    public string StudentNumber { get; set; } = string.Empty;

    public Guid? MajorId { get; set; }

    public string PersonnelCode { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public sealed record MajorLookupVm(
    Guid Id,
    string Title);

public sealed class ApiErrorResponse
{
    public string[] Errors { get; set; } = [];
}