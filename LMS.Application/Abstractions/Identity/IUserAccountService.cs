using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Models;
using Common.Enums;

namespace Application.Abstractions.Identity;

public interface IUserAccountService
{
    Task<UserAccountDto?> FindByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<UserAccountDto?> FindByNationalCodeAsync(
        string nationalCode,
        CancellationToken cancellationToken = default);

    Task<bool> UserNameExistsAsync(
        string userName,
        Guid? excludingUserId = null,
        CancellationToken cancellationToken = default);

    Task<bool> PhoneNumberExistsAsync(
        string phoneNumber,
        Guid? excludingUserId = null,
        CancellationToken cancellationToken = default);

    Task<IdentityOperationResult> CreateAsync(
        UserIdentityData data,
        string password,
        CancellationToken cancellationToken = default);

    Task<IdentityOperationResult> UpdateAsync(
        Guid userId,
        string firstName,
        string lastName,
        string phoneNumber,
        string? email,
        string? latinFirstName,
        string? latinLastName,
        Gender gender,
        string? profileImagePath,
        CancellationToken cancellationToken = default);

    Task<IdentityOperationResult> SetActiveAsync(
        Guid userId,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<IdentityOperationResult> AddToRoleAsync(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken = default);

    Task<IdentityOperationResult> DeleteAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}