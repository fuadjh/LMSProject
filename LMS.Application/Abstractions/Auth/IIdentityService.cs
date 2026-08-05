using Common.Contracts.Auth;


namespace Application.Abstractions.Auth;

public interface IIdentityService
{
    Task<Guid> CreateUserAsync(
        string userName,
        string email,
        string password,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default);

    Task<Guid?> ValidateCredentialsAsync(
        string userName,
        string password,
        CancellationToken cancellationToken = default);

    Task<SignInDataDto?> GetSignInDataAsync(
        Guid authUserId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task SetUserRolesAsync(
        Guid authUserId,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<string>> GetUserRolesAsync(
        Guid authUserId,
        CancellationToken cancellationToken = default);

    Task AddUserToRoleAsync(
    Guid authUserId,
    string role,
    CancellationToken cancellationToken = default);

    Task UpdateUserEmailAsync(
        Guid authUserId,
        string email,
        CancellationToken cancellationToken = default);

    Task SetUserActiveStatusAsync(
        Guid authUserId,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task DeleteUserAsync(
        Guid authUserId,
        CancellationToken cancellationToken = default);
}