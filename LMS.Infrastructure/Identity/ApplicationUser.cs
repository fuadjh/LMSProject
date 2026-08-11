using Common.Enums;
using Common.Validation;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string NationalCode { get; private set; } = string.Empty;

    public string? LatinFirstName { get; private set; }

    public string? LatinLastName { get; private set; }

    public Gender Gender { get; private set; }

    public string? ProfileImagePath { get; private set; }

    public bool IsActive { get; private set; } = true;

    public static ApplicationUser Create(
        string userName,
        string firstName,
        string lastName,
        string nationalCode,
        string phoneNumber,
        string? email,
        string? latinFirstName,
        string? latinLastName,
        Gender gender,
        string? profileImagePath)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("نام کاربری الزامی است.");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = IranianIdentityNormalizer.NormalizeUserName(userName),
            NationalCode =
                IranianIdentityNormalizer.NormalizeNationalCode(nationalCode),
            IsActive = true
        };

        user.UpdateProfile(
            firstName,
            lastName,
            phoneNumber,
            email,
            latinFirstName,
            latinLastName,
            gender,
            profileImagePath);

        return user;
    }

    public void UpdateProfile(
        string firstName,
        string lastName,
        string phoneNumber,
        string? email,
        string? latinFirstName,
        string? latinLastName,
        Gender gender,
        string? profileImagePath)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("نام الزامی است.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("نام خانوادگی الزامی است.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();

        PhoneNumber =
            IranianIdentityNormalizer.NormalizeMobile(phoneNumber);

        Email = string.IsNullOrWhiteSpace(email)
            ? null
            : email.Trim();

        LatinFirstName = NormalizeOptional(latinFirstName);
        LatinLastName = NormalizeOptional(latinLastName);
        Gender = gender;
        ProfileImagePath = NormalizeOptional(profileImagePath);
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}