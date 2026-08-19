using Domain.Common;

namespace Domain.Entities.Users;

public sealed class UserProfile : BaseEntity
{
    private UserProfile()
    {
    }

    public Guid AuthUserId { get; private set; }

    public string? NationalCode { get; private set; }

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public static UserProfile Create(
        Guid authUserId,
        string firstName,
        string lastName,
        string? nationalCode = null)
    {
        if (authUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "شناسه حساب کاربری الزامی است.",
                nameof(authUserId));
        }

        ValidateName(firstName, nameof(firstName));
        ValidateName(lastName, nameof(lastName));

        string? normalizedNationalCode = null;

        if (!string.IsNullOrWhiteSpace(nationalCode))
        {
            normalizedNationalCode =
                NormalizeNationalCode(nationalCode);

            if (!IsValidNationalCode(normalizedNationalCode))
            {
                throw new ArgumentException(
                    "کد ملی معتبر نیست.",
                    nameof(nationalCode));
            }
        }

        return new UserProfile
        {
            Id = Guid.NewGuid(),
            AuthUserId = authUserId,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            NationalCode = normalizedNationalCode,
            IsActive = true
        };
    }

    public void UpdateName(
        string firstName,
        string lastName)
    {
        ValidateName(firstName, nameof(firstName));
        ValidateName(lastName, nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public static string NormalizeNationalCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var characters = value
            .Trim()
            .Where(character =>
                !char.IsWhiteSpace(character) &&
                character != '-')
            .Select(NormalizeDigit)
            .ToArray();

        return new string(characters);
    }

    public static bool IsValidNationalCode(string? value)
    {
        var nationalCode =
            NormalizeNationalCode(value ?? string.Empty);

        if (nationalCode.Length != 10 ||
            nationalCode.Any(character =>
                character is < '0' or > '9') ||
            nationalCode.Distinct().Count() == 1)
        {
            return false;
        }

        var sum = 0;

        for (var index = 0; index < 9; index++)
        {
            sum +=
                (nationalCode[index] - '0') *
                (10 - index);
        }

        var remainder = sum % 11;

        var expectedCheckDigit =
            remainder < 2
                ? remainder
                : 11 - remainder;

        return nationalCode[9] - '0' ==
               expectedCheckDigit;
    }

    private static void ValidateName(
        string value,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "نام و نام خانوادگی الزامی است.",
                parameterName);
        }
    }

    private static char NormalizeDigit(char value)
    {
        return value switch
        {
            '۰' or '٠' => '0',
            '۱' or '١' => '1',
            '۲' or '٢' => '2',
            '۳' or '٣' => '3',
            '۴' or '٤' => '4',
            '۵' or '٥' => '5',
            '۶' or '٦' => '6',
            '۷' or '٧' => '7',
            '۸' or '٨' => '8',
            '۹' or '٩' => '9',
            _ => value
        };
    }
}