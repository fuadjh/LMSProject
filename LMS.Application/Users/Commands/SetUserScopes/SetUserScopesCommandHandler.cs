using Application.Abstractions.Persistence;
using Application.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.SetUserScopes;

public sealed class SetUserScopesCommandHandler
    : IRequestHandler<SetUserScopesCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;

    public SetUserScopesCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(SetUserScopesCommand request, CancellationToken cancellationToken)
    {
        var facultyIds = request.FacultyIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();

        var majorIds = request.MajorIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();

        var userProfile = await _dbContext.UserProfiles
            .Include(x => x.FacultyScopes)
            .Include(x => x.MajorScopes)
            .SingleOrDefaultAsync(x => x.Id == request.UserProfileId, cancellationToken);

        if (userProfile is null)
            throw new InvalidOperationException("User profile not found.");

        var existingFacultyIds = await _dbContext.Faculties
            .Where(x => facultyIds.Contains(x.Id) && x.IsActive)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var invalidFacultyIds = facultyIds.Except(existingFacultyIds).ToArray();
        if (invalidFacultyIds.Any())
            throw new InvalidOperationException("One or more faculty ids are invalid.");

        var existingMajors = await _dbContext.Majors
            .Where(x => majorIds.Contains(x.Id) && x.IsActive)
            .Select(x => new { x.Id, x.FacultyId })
            .ToListAsync(cancellationToken);

        var invalidMajorIds = majorIds.Except(existingMajors.Select(x => x.Id)).ToArray();
        if (invalidMajorIds.Any())
            throw new InvalidOperationException("One or more major ids are invalid.");

        userProfile.SetFacultyScopes(facultyIds);
        userProfile.SetMajorScopes(majorIds);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}