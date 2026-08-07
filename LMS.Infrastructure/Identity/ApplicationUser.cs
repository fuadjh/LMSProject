using Common.Enums;
using Common.Validation;
using Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    private ApplicationUser()
    {
    }

    public string FirstName { get; private set; } = null!;

    public string LastName { get; private set; } = null!;

    public string NationalCode { get; private set; } = null!;

    public string? LatinFirstName { get; private set; }

    public string? LatinLastName { get; private set; }

    public Gender Gender { get; private set; }

    public string? ProfileImagePath { get; private set; }

    public bool IsActive { get; private set; } = true;

    public StudentProfile? StudentProfile { get; private set; }

    public InstructorProfile? InstructorProfile { get; private set; }

    public ExpertProfile? ExpertProfile { get; private set; }

    public ICollection<UserFacultyScope> FacultyScopes { get; private set; } =
        new List<UserFacultyScope>();

    public ICollection<UserMajorScope> MajorScopes { get; private set; } =
        new List<UserMajorScope>();

    public static ApplicationUser Create(
        string userName,
        string firstName,
        string lastName,
        string nationalCode,
        string mobile,
        string? email,
        string? latinFirstName,
        string? latinLastName,
        Gender gender,
        string? profileImagePath)
    {
        ValidateProfile(
            userName,
            firstName,
            lastName,
            nationalCode,
            mobile,
            gender);

        var normalizedNationalCode =
            IranianIdentityNormalizer.NormalizeNationalCode(nationalCode);

        var normalizedMobile =
            IranianIdentityNormalizer.NormalizeMobile(mobile);

        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName =
                IranianIdentityNormalizer.NormalizeUserName(userName),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            NationalCode = normalizedNationalCode,
            PhoneNumber = normalizedMobile,
            Email = NormalizeOptional(email),
            LatinFirstName = NormalizeOptional(latinFirstName),
            LatinLastName = NormalizeOptional(latinLastName),
            Gender = gender,
            ProfileImagePath = NormalizeOptional(profileImagePath),
            IsActive = true,
            PhoneNumberConfirmed = true
        };
    }

    public void UpdateProfile(
        string firstName,
        string lastName,
        string mobile,
        string? email,
        string? latinFirstName,
        string? latinLastName,
        Gender gender,
        string? profileImagePath)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException(
                "نام الزامی است.",
                nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException(
                "نام خانوادگی الزامی است.",
                nameof(lastName));
        }

        if (!IranianIdentityNormalizer.IsValidMobile(mobile))
        {
            throw new ArgumentException(
                "شماره موبایل معتبر نیست.",
                nameof(mobile));
        }

        if (!Enum.IsDefined(gender))
        {
            throw new ArgumentOutOfRangeException(nameof(gender));
        }

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        PhoneNumber = IranianIdentityNormalizer.NormalizeMobile(mobile);
        Email = NormalizeOptional(email);
        LatinFirstName = NormalizeOptional(latinFirstName);
        LatinLastName = NormalizeOptional(latinLastName);
        Gender = gender;
        ProfileImagePath = NormalizeOptional(profileImagePath);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private static void ValidateProfile(
        string userName,
        string firstName,
        string lastName,
        string nationalCode,
        string mobile,
        Gender gender)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException(
                "نام کاربری الزامی است.",
                nameof(userName));
        }

        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException(
                "نام الزامی است.",
                nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException(
                "نام خانوادگی الزامی است.",
                nameof(lastName));
        }

        if (!IranianIdentityNormalizer.IsValidNationalCode(nationalCode))
        {
            throw new ArgumentException(
                "کد ملی معتبر نیست.",
                nameof(nationalCode));
        }

        if (!IranianIdentityNormalizer.IsValidMobile(mobile))
        {
            throw new ArgumentException(
                "شماره موبایل معتبر نیست.",
                nameof(mobile));
        }

        if (!Enum.IsDefined(gender))
        {
            throw new ArgumentOutOfRangeException(nameof(gender));
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}