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
            await _dbContext.EducationExpertProfiles
                .AnyAsync(
                    x =>
                        x.EmployeeCode ==
                        request.EmployeeCode.Trim(),
                    cancellationToken);

        if (codeExists)
        {
            return Result<Guid>.Conflict(
                "expert.code_exists",
                "کد کارمندی تکراری است.");
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
                    [RoleNames.EducationExpert],
                    cancellationToken);

            var userProfile = UserProfile.Create(
                authUserId,
                request.FirstName,
                request.LastName);

            var expertProfile =
                EducationExpertProfile.Create(
                    userProfile.Id,
                    request.EmployeeCode);

            await _dbContext.AddAsync(
                userProfile,
                cancellationToken);

            await _dbContext.AddAsync(
                expertProfile,
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
                "education_expert.create_failed",
                ex.Message);
        }
    }
}