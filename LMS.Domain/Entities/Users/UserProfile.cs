using Domain.Common;

namespace Domain.Entities.Users;

public sealed class UserProfile : BaseEntity
{
    private readonly List<UserFacultyScope> _facultyScopes = [];
    private readonly List<UserMajorScope> _majorScopes = [];

    private UserProfile()
    {
    }

    public Guid AuthUserId { get; private set; }

    // برای سازگاری با کاربران قدیمی nullable است.
    public string? NationalCode { get; private set; }

    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<UserFacultyScope> FacultyScopes =>
        _facultyScopes.AsReadOnly();

    public IReadOnlyCollection<UserMajorScope> MajorScopes =>
        _majorScopes.AsReadOnly();

    public static UserProfile Create(
        Guid authUserId,
        string firstName,
        string lastName,
        string? nationalCode = null)
    {
        if (authUserId == Guid.Empty)
            throw new ArgumentException("AuthUserId is required.");

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("FirstName is required.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("LastName is required.");

        string? normalizedNationalCode = null;

        if (!string.IsNullOrWhiteSpace(nationalCode))
        {
            normalizedNationalCode = NormalizeNationalCode(nationalCode);

            if (!IsValidNationalCode(normalizedNationalCode))
                throw new ArgumentException("NationalCode is invalid.");
        }

        return new UserProfile
        {
            Id = Guid.NewGuid(),
            AuthUserId = authUserId,
            NationalCode = normalizedNationalCode,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            IsActive = true
        };
    }

    public void UpdateName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("FirstName is required.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("LastName is required.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void SetFacultyScopes(IEnumerable<Guid> facultyIds)
    {
        _facultyScopes.Clear();

        foreach (var facultyId in facultyIds
                     .Where(x => x != Guid.Empty)
                     .Distinct())
        {
            _facultyScopes.Add(
                UserFacultyScope.Create(Id, facultyId));
        }
    }

    public void SetMajorScopes(IEnumerable<Guid> majorIds)
    {
        _majorScopes.Clear();

        foreach (var majorId in majorIds
                     .Where(x => x != Guid.Empty)
                     .Distinct())
        {
            _majorScopes.Add(
                UserMajorScope.Create(Id, majorId));
        }
    }

    public static string NormalizeNationalCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var characters = value
            .Trim()
            .Where(x => !char.IsWhiteSpace(x) && x != '-')
            .Select(NormalizeDigit)
            .ToArray();

        return new string(characters);
    }

    public static bool IsValidNationalCode(string? value)
    {
        var nationalCode = NormalizeNationalCode(value ?? string.Empty);

        if (nationalCode.Length != 10 ||
            nationalCode.Any(x => x is < '0' or > '9') ||
            nationalCode.Distinct().Count() == 1)
        {
            return false;
        }

        var sum = 0;

        for (var index = 0; index < 9; index++)
        {
            sum += (nationalCode[index] - '0') * (10 - index);
        }

        var remainder = sum % 11;
        var expectedCheckDigit =
            remainder < 2 ? remainder : 11 - remainder;

        return nationalCode[9] - '0' == expectedCheckDigit;
    }

    private static char NormalizeDigit(char value) =>
        value switch
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