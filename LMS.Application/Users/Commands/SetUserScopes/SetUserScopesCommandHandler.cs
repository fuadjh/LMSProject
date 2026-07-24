using Application.Abstractions.Persistence;
using Application.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.SetUserScopes;

public sealed class SetUserScopesCommandHandler
    : IRequestHandler<SetUserScopesCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;

    public SetUserScopesCommandHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(
        SetUserScopesCommand request,
        CancellationToken cancellationToken)
    {
        var facultyIds = request.FacultyIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();

        var majorIds = request.MajorIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();

        var userProfile =
            await _dbContext.UserProfiles
                .Include(x => x.FacultyScopes)
                .Include(x => x.MajorScopes)
                .SingleOrDefaultAsync(
                    x => x.Id == request.UserProfileId,
                    cancellationToken);

        if (userProfile is null)
        {
            return Result.NotFound(
                "user_profile.not_found",
                "پروفایل کاربر یافت نشد.");
        }

        var validFacultyIds =
            await _dbContext.Faculties
                .Where(x =>
                    facultyIds.Contains(x.Id) &&
                    x.IsActive)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

        if (facultyIds.Except(validFacultyIds).Any())
        {
            return Result.Invalid(
                Error.Validation(
                    "facultyIds",
                    "یک یا چند دانشکده معتبر نیست."));
        }

        var validMajorIds =
            await _dbContext.Majors
                .Where(x =>
                    majorIds.Contains(x.Id) &&
                    x.IsActive)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

        if (majorIds.Except(validMajorIds).Any())
        {
            return Result.Invalid(
                Error.Validation(
                    "majorIds",
                    "یک یا چند رشته معتبر نیست."));
        }

        userProfile.SetFacultyScopes(facultyIds);
        userProfile.SetMajorScopes(majorIds);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}