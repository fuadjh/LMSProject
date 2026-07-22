using Application.Abstractions.Security;
using Domain.Entities.Academics;
using Domain.Entities.Users;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class AccessScopeService : IAccessScopeService
{
    private readonly LmsDbContext _dbContext;

    public AccessScopeService(LmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HasFacultyAccessAsync(Guid userProfileId, Guid facultyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<UserFacultyScope>()
            .AnyAsync(x => x.UserProfileId == userProfileId && x.FacultyId == facultyId, cancellationToken);
    }

    public async Task<bool> HasMajorAccessAsync(Guid userProfileId, Guid majorId, CancellationToken cancellationToken = default)
    {
        var directMajorAccess = await _dbContext.Set<UserMajorScope>()
            .AnyAsync(x => x.UserProfileId == userProfileId && x.MajorId == majorId, cancellationToken);

        if (directMajorAccess)
            return true;

        var facultyId = await _dbContext.Set<Major>()
            .Where(x => x.Id == majorId && x.IsActive)
            .Select(x => (Guid?)x.FacultyId)
            .SingleOrDefaultAsync(cancellationToken);

        if (!facultyId.HasValue)
            return false;

        return await HasFacultyAccessAsync(userProfileId, facultyId.Value, cancellationToken);
    }
}