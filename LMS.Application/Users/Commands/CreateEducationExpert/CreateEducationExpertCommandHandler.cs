using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Common.Results;
using Common.Security;
using Domain.Entities.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.CreateEducationExpert;

public sealed class CreateEducationExpertCommandHandler
    : IRequestHandler<
        CreateEducationExpertCommand,
        Result<Guid>>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _dbContext;

    public CreateEducationExpertCommandHandler(
        IIdentityService identityService,
        IApplicationDbContext dbContext)
    {
        _identityService = identityService;
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(
        CreateEducationExpertCommand request,
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

        var normalizedEmployeeCode =
            request.EmployeeCode.Trim();

        var employeeCodeExists =
            await _dbContext.EducationExpertProfiles
                .AnyAsync(
                    x => x.EmployeeCode == normalizedEmployeeCode,
                    cancellationToken);

        if (employeeCodeExists)
        {
            return Result<Guid>.Conflict(
                "education_expert.employee_code_exists",
                "کد کارمندی تکراری است.");
        }

        var existingUserProfile =
            await _dbContext.UserProfiles
                .SingleOrDefaultAsync(
                    x => x.NationalCode == nationalCode,
                    cancellationToken);

        var existingEducationExpertProfile =
            existingUserProfile is null
                ? null
                : await _dbContext.EducationExpertProfiles
                    .SingleOrDefaultAsync(
                        x => x.UserProfileId == existingUserProfile.Id,
                        cancellationToken);

        if (existingEducationExpertProfile is not null)
        {
            return Result<Guid>.Conflict(
                "education_expert.profile_exists",
                "این شخص قبلاً پروفایل کارشناس آموزش دارد.");
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
                    RoleNames.EducationExpert,
                    cancellationToken);
            }
            else
            {
                var authUserId =
                    await _identityService.CreateUserAsync(
                        request.UserName.Trim(),
                        request.Email.Trim(),
                        request.Password,
                        [RoleNames.EducationExpert],
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

            var educationExpertProfile =
                ExpertProfile.Create(
                    userProfile.Id,
                    normalizedEmployeeCode);

            await _dbContext.AddAsync(
                educationExpertProfile,
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
                "education_expert.create_conflict",
                ex.Message);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            return Result<Guid>.Failure(
                "education_expert.create_failed",
                ex.Message);
        }
    }

    private static Result ValidateNewUserCredentials(
        CreateEducationExpertCommand request)
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