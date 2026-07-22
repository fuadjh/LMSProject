using Domain.Common;

namespace Domain.Entities.Users;

public sealed class UserProfile : BaseEntity
{
    private readonly List<UserFacultyScope> _facultyScopes = new();
    private readonly List<UserMajorScope> _majorScopes = new();

    private UserProfile() { }

    public Guid AuthUserId { get; private set; }
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<UserFacultyScope> FacultyScopes => _facultyScopes.AsReadOnly();
    public IReadOnlyCollection<UserMajorScope> MajorScopes => _majorScopes.AsReadOnly();

    public static UserProfile Create(Guid authUserId, string firstName, string lastName)
    {
        if (authUserId == Guid.Empty)
            throw new ArgumentException("AuthUserId is required.");

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("FirstName is required.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("LastName is required.");

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
            _facultyScopes.Add(UserFacultyScope.Create(Id, facultyId));
        }
    }

    public void SetMajorScopes(IEnumerable<Guid> majorIds)
    {
        _majorScopes.Clear();

        foreach (var majorId in majorIds
                     .Where(x => x != Guid.Empty)
                     .Distinct())
        {
            _majorScopes.Add(UserMajorScope.Create(Id, majorId));
        }
    }
}