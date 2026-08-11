using Application.Abstractions.Identity;
using Application.Common.Models;
using Common.Enums;
using Common.Validation;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class UserAccountService : IUserAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserAccountService(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
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

   
}