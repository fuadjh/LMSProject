using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Common.Results;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.SetUserActiveStatus;

public sealed class SetUserActiveStatusCommandHandler
    : IRequestHandler<SetUserActiveStatusCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IIdentityService _identityService;

    public SetUserActiveStatusCommandHandler(
        IApplicationDbContext dbContext,
        IIdentityService identityService)
    {
        _dbContext = dbContext;
        _identityService = identityService;
    }

    public async Task<Result> Handle(
        SetUserActiveStatusCommand request,
        CancellationToken cancellationToken)
    {
        var userProfile =
            await _dbContext.UserProfiles.SingleOrDefaultAsync(
                x => x.Id == request.UserProfileId,
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
            if (request.IsActive)
                userProfile.Activate();
            else
                userProfile.Deactivate();

            await _identityService.SetUserActiveStatusAsync(
                userProfile.AuthUserId,
                request.IsActive,
                cancellationToken);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            return Result.Failure(
                "user.active_status_failed",
                ex.Message);
        }
    }
}

public sealed class SetUserActiveStatusCommandValidator
    : AbstractValidator<SetUserActiveStatusCommand>
{
    public SetUserActiveStatusCommandValidator()
    {
        RuleFor(x => x.UserProfileId)
            .NotEmpty();
    }
}