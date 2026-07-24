using Application.Abstractions.Auth;
using Common.Contracts.Auth;
using Common.Security;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly LmsDbContext _dbContext;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        LmsDbContext dbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
    }

    public async Task<Guid> CreateUserAsync(
        string userName,
        string email,
        string password,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var requestedRoles = roles
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var role in requestedRoles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                throw new InvalidOperationException(
                    $"Role '{role}' does not exist.");
            }
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = userName.Trim(),
            Email = email.Trim(),
            EmailConfirmed = true,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(
            user,
            password);

        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    " | ",
                    createResult.Errors.Select(x => x.Description)));
        }

        if (requestedRoles.Length > 0)
        {
            var roleResult = await _userManager.AddToRolesAsync(
                user,
                requestedRoles);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(
                        " | ",
                        roleResult.Errors.Select(x => x.Description)));
            }
        }

        return user.Id;
    }

    public async Task<Guid?> ValidateCredentialsAsync(
        string userName,
        string password,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(userName) ||
            string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var user = await _userManager.FindByNameAsync(
            userName.Trim());

        cancellationToken.ThrowIfCancellationRequested();

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var passwordIsValid = await _userManager.CheckPasswordAsync(
            user,
            password);

        return passwordIsValid
            ? user.Id
            : null;
    }

    public async Task<SignInDataDto?> GetSignInDataAsync(
        Guid authUserId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(
            authUserId.ToString());

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var userProfile = await _dbContext.UserProfiles
            .AsNoTracking()
            .Where(x => x.AuthUserId == user.Id)
            .Select(x => new
            {
                x.Id,
                DisplayName = x.FirstName + " " + x.LastName
            })
            .SingleOrDefaultAsync(cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);

        cancellationToken.ThrowIfCancellationRequested();

        var permissions = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        var userClaims = await _userManager.GetClaimsAsync(user);

        foreach (var claim in userClaims.Where(
                     x => x.Type == CustomClaimTypes.Permission))
        {
            permissions.Add(claim.Value);
        }

        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role is null)
            {
                continue;
            }

            var roleClaims = await _roleManager.GetClaimsAsync(role);

            foreach (var claim in roleClaims.Where(
                         x => x.Type == CustomClaimTypes.Permission))
            {
                permissions.Add(claim.Value);
            }
        }

        return new SignInDataDto
        {
            AuthUserId = user.Id,
            UserProfileId = userProfile?.Id,
            UserName = user.UserName ?? string.Empty,
            DisplayName = userProfile?.DisplayName,
            Roles = roles
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            Permissions = permissions
                .OrderBy(x => x)
                .ToArray()
        };
    }

    public async Task<bool> ExistsByUserNameAsync(
        string userName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(userName))
        {
            return false;
        }

        return await _userManager.FindByNameAsync(
            userName.Trim()) is not null;
    }

    public async Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        return await _userManager.FindByEmailAsync(
            email.Trim()) is not null;
    }

    public async Task SetUserRolesAsync(
        Guid authUserId,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(
            authUserId.ToString())
            ?? throw new InvalidOperationException(
                "User not found.");

        var requestedRoles = roles
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var role in requestedRoles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                throw new InvalidOperationException(
                    $"Role '{role}' does not exist.");
            }
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        var toAdd = requestedRoles
            .Except(
                currentRoles,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var toRemove = currentRoles
            .Except(
                requestedRoles,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (toRemove.Length > 0)
        {
            var removeResult =
                await _userManager.RemoveFromRolesAsync(
                    user,
                    toRemove);

            if (!removeResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(
                        " | ",
                        removeResult.Errors.Select(
                            x => x.Description)));
            }
        }

        if (toAdd.Length > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(
                user,
                toAdd);

            if (!addResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(
                        " | ",
                        addResult.Errors.Select(
                            x => x.Description)));
            }
        }
    }

    public async Task<IReadOnlyCollection<string>> GetUserRolesAsync(
        Guid authUserId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(
            authUserId.ToString())
            ?? throw new InvalidOperationException(
                "User not found.");

        var roles = await _userManager.GetRolesAsync(user);

        return roles
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToArray();
    }
}