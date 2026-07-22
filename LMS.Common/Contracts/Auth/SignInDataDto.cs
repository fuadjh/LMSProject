namespace Common.Contracts.Auth;

public sealed class SignInDataDto
{
    public Guid AuthUserId { get; set; }
    public Guid? UserProfileId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }

    public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
    public IReadOnlyCollection<string> Permissions { get; set; } = Array.Empty<string>();
}