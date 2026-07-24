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
                "رشته یافت نشد.");
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
                    "پروفایل کاربر یافت نشد.");
            }

            var hasScope =
                await _scopeService.HasMajorAccessAsync(
                    _currentUser.UserProfileId.Value,
                    request.MajorId,
                    cancellationToken);

            if (!hasScope)
            {
                return Result<Guid>.Forbidden(
                    "scope.denied",
                    "دسترسی به این رشته را ندارید.");
            }
        }

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

        var studentNumberExists =
            await _dbContext.StudentProfiles.AnyAsync(
                x =>
                    x.StudentNumber ==
                    request.StudentNumber.Trim(),
                cancellationToken);

        if (studentNumberExists)
        {
            return Result<Guid>.Conflict(
                "student.number_exists",
                "شماره دانشجویی تکراری است.");
        }

        await using var transaction =
            await _dbContext.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var authUserId =
                await _identityService.CreateUserAsync(
                    request.UserName.Trim(),
                    request.Email.Trim(),
                    request.Password,
                    [RoleNames.Student],
                    cancellationToken);

            var userProfile = UserProfile.Create(
                authUserId,
                request.FirstName,
                request.LastName);

            var studentProfile =
                StudentProfile.Create(
                    userProfile.Id,
                    request.StudentNumber,
                    request.MajorId);

            await _dbContext.AddAsync(
                userProfile,
                cancellationToken);

            await _dbContext.AddAsync(
                studentProfile,
                cancellationToken);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return Result<Guid>.Success(userProfile.Id);
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
}