using Common.Enums;

namespace Application.Common.Models;

public sealed record UserIdentityData(
    string UserName,
    string FirstName,
    string LastName,
    string NationalCode,
    string PhoneNumber,
    string? Email,
    string? LatinFirstName,
    string? LatinLastName,
    Gender Gender,
    string? ProfileImagePath);

public sealed record UserAccountDto(
    Guid Id,
    string UserName,
    string FirstName,
    string LastName,
    string NationalCode,
    string PhoneNumber,
    string? Email,
    string? LatinFirstName,
    string? LatinLastName,
    Gender Gender,
    string? ProfileImagePath,
    bool IsActive);

public sealed record IdentityOperationResult(
    bool Succeeded,
    Guid? UserId,
    IReadOnlyCollection<string> Errors)
{
    public static IdentityOperationResult Success(Guid userId)
    {
        return new IdentityOperationResult(
            true,
            userId,
            Array.Empty<string>());
    }

    public static IdentityOperationResult Failure(
        params string[] errors)
    {
        return new IdentityOperationResult(
            false,
            null,
            errors);
    }
}