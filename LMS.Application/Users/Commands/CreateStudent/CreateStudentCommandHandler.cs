using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Abstractions.Security;
using Application.Common.Results;
using Common.Security;
using Domain.Entities.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.CreateStudent;

public sealed class CreateStudentCommandHandler
    : IRequestHandler<CreateStudentCommand, Result<Guid>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IIdentityService _identityService;
    private readonly IAccessScopeService _scopeService;
    private readonly IApplicationDbContext _dbContext;

    public CreateStudentCommandHandler(
        ICurrentUser currentUser,
        IIdentityService identityService,
        IAccessScopeService scopeService,
        IApplicationDbContext dbContext)
    {
        _currentUser = currentUser;
        _identityService = identityService;
        _scopeService = scopeService;
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(
        CreateStudentCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result<Guid>.Unauthorized(
                "auth.required",
                "کاربر احراز هویت نشده است.");
        }

        var nationalCode =
            UserProfile.NormalizeNationalCode(request.NationalCode);

        if (!UserProfile.IsValidNationalCode(nationalCode))
        {
            return Result<Guid>.Invalid(
                Error.Validation(
                    "nationalCode",
                    "کد ملی معتبر نیست."));
        }

        var majorExists =
            await _dbContext.Majors.AnyAsync(
                x =>
                    x.Id == request.MajorId &&
                    x.IsActive,
                cancellationToken);

        if (!majorExists)
        {
            return Result<Guid>.NotFound(
                "major.not_found",
                "رشته فعال یافت نشد.");
        }

        var isAdmin = _currentUser.Roles.Contains(
            RoleNames.Admin,
            StringComparer.OrdinalIgnoreCase);

        if (!isAdmin)
        {
            if (!_currentUser.UserProfileId.HasValue)
            {
                return Result<Guid>.Forbidden(
                    "scope.user_profile_required",
                    "پروفایل کاربر جاری یافت نشد.");
            }

            var hasMajorAccess =
                await _scopeService.HasMajorAccessAsync(
                    _currentUser.UserProfileId.Value,
                    request.MajorId,
                    cancellationToken);

            if (!hasMajorAccess)
            {
                return Result<Guid>.Forbidden(
                    "scope.denied",
                    "دسترسی به این رشته را ندارید.");
            }
        }

        var normalizedStudentNumber =
            request.StudentNumber.Trim();

        var studentNumberExists =
            await _dbContext.StudentProfiles.AnyAsync(
                x => x.StudentNumber == normalizedStudentNumber,
                cancellationToken);

        if (studentNumberExists)
        {
            return Result<Guid>.Conflict(
                "student.number_exists",
                "شماره دانشجویی تکراری است.");
        }

        var existingUserProfile =
            await _dbContext.UserProfiles
                .SingleOrDefaultAsync(
                    x => x.NationalCode == nationalCode,
                    cancellationToken);

        var existingStudentProfile =
            existingUserProfile is null
                ? null
                : await _dbContext.StudentProfiles
                    .SingleOrDefaultAsync(
                        x => x.UserProfileId == existingUserProfile.Id,
                        cancellationToken);

        if (existingStudentProfile is not null)
        {
            return Result<Guid>.Conflict(
                "student.profile_exists",
                "این شخص قبلاً پروفایل دانشجویی دارد.");
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
                    RoleNames.Student,
                    cancellationToken);
            }
            else
            {
                var authUserId =
                    await _identityService.CreateUserAsync(
                        request.UserName.Trim(),
                        request.Email.Trim(),
                        request.Password,
                        [RoleNames.Student],
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

            var studentProfile =
                StudentProfile.Create(
                    userProfile.Id,
                    normalizedStudentNumber,
                    request.MajorId);

            await _dbContext.AddAsync(
                studentProfile,
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
                "student.create_conflict",
                ex.Message);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            return Result<Guid>.Failure(
                "student.create_failed",
                ex.Message);
        }
    }

    private static Result ValidateNewUserCredentials(
        CreateStudentCommand request)
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