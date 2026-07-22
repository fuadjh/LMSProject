namespace Common.Contracts.Auth;

public sealed class CurrentUserDto
{
    public bool IsAuthenticated { get; set; }
    public Guid? AuthUserId { get; set; }
    public Guid? UserProfileId { get; set; }
    public string? UserName { get; set; }

    public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
    public IReadOnlyCollection<string> Permissions { get; set; } = Array.Empty<string>();
}