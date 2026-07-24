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

        var codeExists =
            await _dbContext.InstructorProfiles.AnyAsync(
                x =>
                    x.PersonnelCode ==
                    request.PersonnelCode.Trim(),
                cancellationToken);

        if (codeExists)
        {
            return Result<Guid>.Conflict(
                "instructor.code_exists",
                "کد پرسنلی تکراری است.");
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
                    [RoleNames.Instructor],
                    cancellationToken);

            var userProfile = UserProfile.Create(
                authUserId,
                request.FirstName,
                request.LastName);

            var instructorProfile =
                InstructorProfile.Create(
                    userProfile.Id,
                    request.PersonnelCode);

            await _dbContext.AddAsync(
                userProfile,
                cancellationToken);

            await _dbContext.AddAsync(
                instructorProfile,
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
                "instructor.create_failed",
                ex.Message);
        }
    }
}