using Application.Abstractions.Identity;
using Application.Common.Models;
using Common.Enums;
using Common.Validation;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
namespace Infrastructure.Services;

public sealed class UserAccountService : IUserAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    public UserAccountService(
     UserManager<ApplicationUser> userManager,
     RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }
    public Task<UserAccountDto?> FindByIdAsync(
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        return Project(_userManager.Users.AsNoTracking())
            .SingleOrDefaultAsync(
                x => x.Id == userId,
                cancellationToken);
    }

    public Task<UserAccountDto?> FindByNationalCodeAsync(
        string nationalCode,
        CancellationToken cancellationToken = default)
    {
        var normalized =
            IranianIdentityNormalizer.NormalizeNationalCode(nationalCode);

        return Project(
                _userManager.Users
                    .AsNoTracking()
                    .Where(x => x.NationalCode == normalized))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<UserAccountDto> Project(
      IQueryable<ApplicationUser> query)
    {
        return query.Select(user => new UserAccountDto(
            user.Id,
            user.UserName ?? string.Empty,
            user.FirstName,
            user.LastName,
            user.NationalCode,
            user.PhoneNumber ?? string.Empty,
            user.Email,
            user.LatinFirstName,
            user.LatinLastName,
            user.Gender,
            user.ProfileImagePath,
            user.IsActive));
    }

    public Task<bool> UserNameExistsAsync(
        string userName,
        Guid? excludingUserId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized =
            _userManager.NormalizeName(
                IranianIdentityNormalizer.NormalizeUserName(userName));

        return _userManager.Users.AnyAsync(
            user =>
                user.NormalizedUserName == normalized &&
                (!excludingUserId.HasValue ||
                 user.Id != excludingUserId.Value),
            cancellationToken);
    }

    public Task<bool> PhoneNumberExistsAsync(
        string phoneNumber,
        Guid? excludingUserId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized =
            IranianIdentityNormalizer.NormalizeMobile(phoneNumber);

        return _userManager.Users.AnyAsync(
            user =>
                user.PhoneNumber == normalized &&
                (!excludingUserId.HasValue ||
                 user.Id != excludingUserId.Value),
            cancellationToken);
    }

    public async Task<IdentityOperationResult> CreateAsync(
        UserIdentityData data,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = ApplicationUser.Create(
            data.UserName,
            data.FirstName,
            data.LastName,
            data.NationalCode,
            data.PhoneNumber,
            data.Email,
            data.LatinFirstName,
            data.LatinLastName,
            data.Gender,
            data.ProfileImagePath);

        var result = await _userManager.CreateAsync(user, password);

        return result.Succeeded
            ? IdentityOperationResult.Success(user.Id)
            : IdentityOperationResult.Failure(
                result.Errors.Select(error => error.Description).ToArray());
    }

    public async Task<IdentityOperationResult> UpdateAsync(
        Guid userId,
        string firstName,
        string lastName,
        string phoneNumber,
        string? email,
        string? latinFirstName,
        string? latinLastName,
        Gender gender,
        string? profileImagePath,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return IdentityOperationResult.Failure(
                "کاربر پیدا نشد.");
        }

        user.UpdateProfile(
            firstName,
            lastName,
            phoneNumber,
            email,
            latinFirstName,
            latinLastName,
            gender,
            profileImagePath);

        var result = await _userManager.UpdateAsync(user);

        return result.Succeeded
            ? IdentityOperationResult.Success(user.Id)
            : IdentityOperationResult.Failure(
                result.Errors.Select(error => error.Description).ToArray());
    }

    public async Task<IdentityOperationResult> SetActiveAsync(
        Guid userId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return IdentityOperationResult.Failure(
                "کاربر پیدا نشد.");
        }

        if (isActive)
        {
            user.Activate();
        }
        else
        {
            user.Deactivate();
        }

        var result = await _userManager.UpdateAsync(user);

        return result.Succeeded
            ? IdentityOperationResult.Success(user.Id)
            : IdentityOperationResult.Failure(
                result.Errors.Select(error => error.Description).ToArray());
    }

    public async Task<IdentityOperationResult> AddToRoleAsync(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return IdentityOperationResult.Failure(
                "کاربر پیدا نشد.");
        }

        if (await _userManager.IsInRoleAsync(user, roleName))
        {
            return IdentityOperationResult.Success(user.Id);
        }

        var result = await _userManager.AddToRoleAsync(user, roleName);

        return result.Succeeded
            ? IdentityOperationResult.Success(user.Id)
            : IdentityOperationResult.Failure(
                result.Errors.Select(error => error.Description).ToArray());
    }

    public async Task<IdentityOperationResult> DeleteAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return IdentityOperationResult.Failure(
                "کاربر پیدا نشد.");
        }

        var result = await _userManager.DeleteAsync(user);

        return result.Succeeded
            ? IdentityOperationResult.Success(user.Id)
            : IdentityOperationResult.Failure(
                result.Errors.Select(error => error.Description).ToArray());
    }
    public async Task<IReadOnlyCollection<string>> GetRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(
            userId.ToString());

        if (user is null)
            return Array.Empty<string>();

        var roles = await _userManager.GetRolesAsync(user);

        return roles
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToArray();
    }

    public async Task<IdentityOperationResult> SetRolesAsync(
        Guid userId,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(
            userId.ToString());

        if (user is null)
        {
            return IdentityOperationResult.Failure(
                "کاربر پیدا نشد.");
        }

        var requestedRoles = roles
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var invalidRoles = new List<string>();

        foreach (var roleName in requestedRoles)
        {
            var roleExists = await _roleManager.RoleExistsAsync(
                roleName);

            if (!roleExists)
                invalidRoles.Add(roleName);
        }

        if (invalidRoles.Count > 0)
        {
            return IdentityOperationResult.Failure(
                $"Roleهای زیر وجود ندارند: {string.Join(", ", invalidRoles)}");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        var rolesToRemove = currentRoles
            .Except(
                requestedRoles,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var rolesToAdd = requestedRoles
            .Except(
                currentRoles,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (rolesToRemove.Length > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(
                user,
                rolesToRemove);

            if (!removeResult.Succeeded)
            {
                return IdentityOperationResult.Failure(
                    removeResult.Errors
                        .Select(x => x.Description)
                        .ToArray());
            }
        }

        if (rolesToAdd.Length > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(
                user,
                rolesToAdd);

            if (!addResult.Succeeded)
            {
                return IdentityOperationResult.Failure(
                    addResult.Errors
                        .Select(x => x.Description)
                        .ToArray());
            }
        }

        return IdentityOperationResult.Success(user.Id);
    }

}