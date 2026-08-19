using Application.Abstractions.Security;
using Domain.Entities.Academics;
using Domain.Entities.Users;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class AccessScopeService
    : IAccessScopeService
{
    private readonly LmsDbContext _dbContext;

    public AccessScopeService(
        LmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> HasFacultyAccessAsync(
        Guid userId,
        Guid facultyId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty ||
            facultyId == Guid.Empty)
        {
            return Task.FromResult(false);
        }

        return _dbContext
            .Set<UserFacultyScope>()
            .AsNoTracking()
            .AnyAsync(
                scope =>
                    scope.UserId == userId &&
                    scope.FacultyId == facultyId,
                cancellationToken);
    }

    public async Task<bool> HasMajorAccessAsync(
        Guid userId,
        Guid majorId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty ||
            majorId == Guid.Empty)
        {
            return false;
        }

        var hasDirectMajorAccess =
            await _dbContext
                .Set<UserMajorScope>()
                .AsNoTracking()
                .AnyAsync(
                    scope =>
                        scope.UserId == userId &&
                        scope.MajorId == majorId,
                    cancellationToken);

        if (hasDirectMajorAccess)
        {
            return true;
        }

        var facultyId =
            await _dbContext
                .Set<Major>()
                .AsNoTracking()
                .Where(major =>
                    major.Id == majorId &&
                    major.IsActive)
                .Select(major => (Guid?)major.FacultyId)
                .SingleOrDefaultAsync(cancellationToken);

        if (!facultyId.HasValue)
        {
            return false;
        }

        return await HasFacultyAccessAsync(
            userId,
            facultyId.Value,
            cancellationToken);
    }
}