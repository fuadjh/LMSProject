using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Common.Models;
using Application.Common.Results;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.UpdateUserProfiles;

public sealed record UpdateStudentCommand(
    Guid UserProfileId,
    string FirstName,
    string LastName,
    string Email) : IRequest<Result>;

public sealed record UpdateInstructorCommand(
    Guid UserProfileId,
    string FirstName,
    string LastName,
    string Email) : IRequest<Result>;

public sealed record UpdateEducationExpertCommand(
    Guid UserProfileId,
    string FirstName,
    string LastName,
    string Email) : IRequest<Result>;

public sealed class UpdateUserProfileCommandHandler :
    IRequestHandler<UpdateStudentCommand, Result>,
    IRequestHandler<UpdateInstructorCommand, Result>,
    IRequestHandler<UpdateEducationExpertCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IIdentityService _identityService;

    public UpdateUserProfileCommandHandler(
        IApplicationDbContext dbContext,
        IIdentityService identityService)
    {
        _dbContext = dbContext;
        _identityService = identityService;
    }

    public Task<Result> Handle(
        UpdateStudentCommand request,
        CancellationToken cancellationToken)
    {
        return HandleCoreAsync(
            request.UserProfileId,
            request.FirstName,
            request.LastName,
            request.Email,
            UserProfileType.Student,
            cancellationToken);
    }

    public Task<Result> Handle(
        UpdateInstructorCommand request,
        CancellationToken cancellationToken)
    {
        return HandleCoreAsync(
            request.UserProfileId,
            request.FirstName,
            request.LastName,
            request.Email,
            UserProfileType.Instructor,
            cancellationToken);
    }

    public Task<Result> Handle(
        UpdateEducationExpertCommand request,
        CancellationToken cancellationToken)
    {
        return HandleCoreAsync(
            request.UserProfileId,
            request.FirstName,
            request.LastName,
            request.Email,
            UserProfileType.EducationExpert,
            cancellationToken);
    }

    private async Task<Result> HandleCoreAsync(
        Guid userProfileId,
        string firstName,
        string lastName,
        string email,
        UserProfileType profileType,
        CancellationToken cancellationToken)
    {
        var profileTypeExists = profileType switch
        {
            UserProfileType.Student =>
                await _dbContext.StudentProfiles.AnyAsync(
                    x => x.UserProfileId == userProfileId,
                    cancellationToken),

            UserProfileType.Instructor =>
                await _dbContext.InstructorProfiles.AnyAsync(
                    x => x.UserProfileId == userProfileId,
                    cancellationToken),

            UserProfileType.EducationExpert =>
                await _dbContext.EducationExpertProfiles.AnyAsync(
                    x => x.UserProfileId == userProfileId,
                    cancellationToken),

            _ => false
        };

        if (!profileTypeExists)
        {
            return Result.NotFound(
                "user_profile.type_not_found",
                "پروفایل تخصصی کاربر یافت نشد.");
        }

        var userProfile =
            await _dbContext.UserProfiles.SingleOrDefaultAsync(
                x => x.Id == userProfileId,
                cancellationToken);

        if (userProfile is null)
        {
            return Result.NotFound(
                "user_profile.not_found",
                "پروفایل کاربر یافت نشد.");
        }

        await using var transaction =
            await _dbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            userProfile.UpdateName(
                firstName,
                lastName);

            await _identityService.UpdateUserEmailAsync(
                userProfile.AuthUserId,
                email,
                cancellationToken);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            return Result.Conflict(
                "user.email_conflict",
                ex.Message);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            return Result.Failure(
                "user.update_failed",
                ex.Message);
        }
    }
}

public sealed class UpdateStudentCommandValidator
    : AbstractValidator<UpdateStudentCommand>
{
    public UpdateStudentCommandValidator()
    {
        RuleFor(x => x.UserProfileId)
            .NotEmpty();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);
    }
}

public sealed class UpdateInstructorCommandValidator
    : AbstractValidator<UpdateInstructorCommand>
{
    public UpdateInstructorCommandValidator()
    {
        RuleFor(x => x.UserProfileId)
            .NotEmpty();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);
    }
}

public sealed class UpdateEducationExpertCommandValidator
    : AbstractValidator<UpdateEducationExpertCommand>
{
    public UpdateEducationExpertCommandValidator()
    {
        RuleFor(x => x.UserProfileId)
            .NotEmpty();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);
    }
}