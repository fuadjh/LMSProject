namespace Application.Abstractions.Auth;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    Guid? AuthUserId { get; }
    Guid? UserProfileId { get; }
    string? UserName { get; }
    IReadOnlyCollection<string> Roles { get; }
    bool HasPermission(string permission);
}