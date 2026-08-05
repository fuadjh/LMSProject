using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Common.Results;
using Common.Security;
using Domain.Entities.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.CreateInstructor;

public sealed class CreateInstructorCommandHandler
    : IRequestHandler<CreateInstructorCommand, Result<Guid>>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _dbContext;

    public CreateInstructorCommandHandler(
        IIdentityService identityService,
        IApplicationDbContext dbContext)
    {
        _identityService = identityService;
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(
        CreateInstructorCommand request,
        CancellationToken cancellationToken)
    {
        var nationalCode =
            UserProfile.NormalizeNationalCode(request.NationalCode);

        if (!UserProfile.IsValidNationalCode(nationalCode))
        {
            return Result<Guid>.Invalid(
                Error.Validation(
                    "nationalCode",
                    "کد ملی معتبر نیست."));
        }

        var normalizedPersonnelCode =
            request.PersonnelCode.Trim();

        var personnelCodeExists =
            await _dbContext.InstructorProfiles.AnyAsync(
                x => x.PersonnelCode == normalizedPersonnelCode,
                cancellationToken);

        if (personnelCodeExists)
        {
            return Result<Guid>.Conflict(
                "instructor.personnel_code_exists",
                "کد پرسنلی تکراری است.");
        }

        var existingUserProfile =
            await _dbContext.UserProfiles
                .SingleOrDefaultAsync(
                    x => x.NationalCode == nationalCode,
                    cancellationToken);

        var existingInstructorProfile =
            existingUserProfile is null
                ? null
                : await _dbContext.InstructorProfiles
                    .SingleOrDefaultAsync(
                        x => x.UserProfileId == existingUserProfile.Id,
                        cancellationToken);

        if (existingInstructorProfile is not null)
        {
            return Result<Guid>.Conflict(
                "instructor.profile_exists",
                "این شخص قبلاً پروفایل استادی دارد.");
        }

        if (existingUserProfile is null)
        {
            var credentialsResult =
                ValidateNewUserCredentials(request);

            if (!credentialsResult.IsSuccess)
                return Result<Guid>.Invalid(
                    credentialsResult.Errors);

            if (await _identityService.ExistsByUserNameAsync(
                    request.UserName,
                    cancellationToken))
            {
                return Result<Guid>.Conflict(
                    "user.username_exists",
                    "نام کاربری تکراری است.");
            }

            if (await _identityService.ExistsByEmailAsync(
                    request.Email,
                    cancellationToken))
            {
                return Result<Guid>.Conflict(
                    "user.email_exists",
                    "ایمیل تکراری است.");
            }
        }

        await using var transaction =
            await _dbContext.BeginTransactionAsync(
                cancellationToken);

        try
        {
            UserProfile userProfile;

            if (existingUserProfile is not null)
            {
                userProfile = existingUserProfile;

                await _identityService.AddUserToRoleAsync(
                    userProfile.AuthUserId,
                    RoleNames.Instructor,
                    cancellationToken);
            }
            else
            {
                var authUserId =
                    await _identityService.CreateUserAsync(
                        request.UserName.Trim(),
                        request.Email.Trim(),
                        request.Password,
                        [RoleNames.Instructor],
                        cancellationToken);

                userProfile = UserProfile.Create(
                    authUserId,
                    request.FirstName,
                    request.LastName,
                    nationalCode);

                await _dbContext.AddAsync(
                    userProfile,
                    cancellationToken);
            }

            var instructorProfile =
                InstructorProfile.Create(
                    userProfile.Id,
                    normalizedPersonnelCode);

            await _dbContext.AddAsync(
                instructorProfile,
                cancellationToken);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return Result<Guid>.Success(
                userProfile.Id);
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            return Result<Guid>.Conflict(
                "instructor.create_conflict",
                ex.Message);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            return Result<Guid>.Failure(
                "instructor.create_failed",
                ex.Message);
        }
    }

    private static Result ValidateNewUserCredentials(
        CreateInstructorCommand request)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            errors.Add(
                Error.Validation(
                    "userName",
                    "نام کاربری برای شخص جدید الزامی است."));
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add(
                Error.Validation(
                    "email",
                    "ایمیل برای شخص جدید الزامی است."));
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors.Add(
                Error.Validation(
                    "password",
                    "رمز عبور برای شخص جدید الزامی است."));
        }

        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            errors.Add(
                Error.Validation(
                    "firstName",
                    "نام برای شخص جدید الزامی است."));
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            errors.Add(
                Error.Validation(
                    "lastName",
                    "نام خانوادگی برای شخص جدید الزامی است."));
        }

        return errors.Count == 0
            ? Result.Success()
            : Result.Invalid(errors);
    }
}