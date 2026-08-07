using Application.Abstractions.Identity;
using Application.Abstractions.Persistence;
using Application.Common.Models;
using Common.Validation;
using Domain.Entities.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.CreateInstructor;

public sealed class CreateInstructorCommandHandler
    : IRequestHandler<CreateInstructorCommand, IdentityOperationResult>
{
    private const string RoleName = "Instructor";

    private readonly IApplicationDbContext _context;
    private readonly IUserAccountService _accounts;

    public CreateInstructorCommandHandler(
        IApplicationDbContext context,
        IUserAccountService accounts)
    {
        _context = context;
        _accounts = accounts;
    }

    public async Task<IdentityOperationResult> Handle(
        CreateInstructorCommand request,
        CancellationToken cancellationToken)
    {
        var personnelCode =
            IranianIdentityNormalizer.NormalizeUserName(
                request.PersonnelCode);

        if (await _context.InstructorProfiles.AnyAsync(
                profile => profile.PersonnelCode == personnelCode,
                cancellationToken))
        {
            return IdentityOperationResult.Failure(
                "کد پرسنلی قبلاً ثبت شده است.");
        }

        var existingUser =
            await _accounts.FindByNationalCodeAsync(
                request.NationalCode,
                cancellationToken);

        Guid userId;
        var createdNewUser = false;

        if (existingUser is null)
        {
            if (await _accounts.PhoneNumberExistsAsync(
                    request.PhoneNumber,
                    null,
                    cancellationToken))
            {
                return IdentityOperationResult.Failure(
                    "شماره موبایل برای کاربر دیگری ثبت شده است.");
            }

            if (await _accounts.UserNameExistsAsync(
                    personnelCode,
                    null,
                    cancellationToken))
            {
                return IdentityOperationResult.Failure(
                    "کد پرسنلی به‌عنوان نام کاربری تکراری است.");
            }

            var createResult = await _accounts.CreateAsync(
                new UserIdentityData(
                    personnelCode,
                    request.FirstName,
                    request.LastName,
                    request.NationalCode,
                    request.PhoneNumber,
                    request.Email,
                    request.LatinFirstName,
                    request.LatinLastName,
                    request.Gender,
                    request.ProfileImagePath),
                request.Password,
                cancellationToken);

            if (!createResult.Succeeded || !createResult.UserId.HasValue)
            {
                return createResult;
            }

            userId = createResult.UserId.Value;
            createdNewUser = true;
        }
        else
        {
            userId = existingUser.Id;

            if (await _context.InstructorProfiles.AnyAsync(
                    profile => profile.Id == userId,
                    cancellationToken))
            {
                return IdentityOperationResult.Failure(
                    "این کد ملی قبلاً دارای نقش استاد است.");
            }
        }

        await using var transaction =
            await _context.BeginTransactionAsync(cancellationToken);

        try
        {
            await _context.AddAsync(
                InstructorProfile.Create(userId, personnelCode),
                cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            var roleResult = await _accounts.AddToRoleAsync(
                userId,
                RoleName,
                cancellationToken);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("، ", roleResult.Errors));
            }

            await transaction.CommitAsync(cancellationToken);

            return IdentityOperationResult.Success(userId);
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync(cancellationToken);

            if (createdNewUser)
            {
                await _accounts.DeleteAsync(userId, cancellationToken);
            }

            return IdentityOperationResult.Failure(exception.Message);
        }
    }
}