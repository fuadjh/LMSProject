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

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<UserFacultyScope> FacultyScopes =>
        _facultyScopes.AsReadOnly();

    public IReadOnlyCollection<UserMajorScope> MajorScopes =>
        _majorScopes.AsReadOnly();

    public static UserProfile Create(
        Guid authUserId,
        string firstName,
        string lastName)
    {
        if (authUserId == Guid.Empty)
            throw new ArgumentException("شناسه کاربر الزامی است.", nameof(authUserId));

        ValidateName(firstName, nameof(firstName));
        ValidateName(lastName, nameof(lastName));

        return new UserProfile
        {
            Id = Guid.NewGuid(),
            AuthUserId = authUserId,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            IsActive = true
        };
    }

    public void UpdateName(string firstName, string lastName)
    {
        ValidateName(firstName, nameof(firstName));
        ValidateName(lastName, nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    public void SetFacultyScopes(IEnumerable<Guid>? facultyIds)
    {
        _facultyScopes.Clear();

        foreach (var facultyId in facultyIds?
                     .Where(x => x != Guid.Empty)
                     .Distinct() ?? [])
        {
            _facultyScopes.Add(
                UserFacultyScope.Create(Id, roleType, facultyId));
        }
    }

    public void SetMajorScopes(IEnumerable<Guid>? majorIds)
    {
        _majorScopes.Clear();

        foreach (var majorId in majorIds?
                     .Where(x => x != Guid.Empty)
                     .Distinct() ?? [])
        {
            _majorScopes.Add(
                UserMajorScope.Create(Id, majorId));
        }
    }

    private static void ValidateName(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("نام و نام خانوادگی الزامی است.", parameterName);

        if (value.Trim().Length > 100)
            throw new ArgumentException("حداکثر طول نام ۱۰۰ کاراکتر است.", parameterName);
    }
}