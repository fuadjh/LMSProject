using Application.Abstractions.Auth;
using Application.Abstractions.Security;
using Common.Security;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class LearningAccessService
    : ILearningAccessService
{
    private readonly ICurrentUser _currentUser;
    private readonly IAccessScopeService _scopeService;
    private readonly LmsDbContext _dbContext;

    public LearningAccessService(
        ICurrentUser currentUser,
        IAccessScopeService scopeService,
        LmsDbContext dbContext)
    {
        _currentUser = currentUser;
        _scopeService = scopeService;
        _dbContext = dbContext;
    }

    public async Task<bool> CanManageOfferingAsync(
        Guid courseOfferingId,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        if (IsInRole(RoleNames.Admin))
            return true;

        if (!IsInRole(RoleNames.Instructor))
            return false;

        var instructorProfileId =
            await GetCurrentInstructorProfileIdAsync(
                cancellationToken);

        if (!instructorProfileId.HasValue)
            return false;

        return await _dbContext.CourseOfferings.AnyAsync(
            x =>
                x.Id == courseOfferingId &&
                x.IsActive &&
                x.InstructorProfileId ==
                instructorProfileId.Value,
            cancellationToken);
    }

    public async Task<bool> CanViewOfferingAsync(
        Guid courseOfferingId,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        if (await CanManageOfferingAsync(
                courseOfferingId,
                cancellationToken))
        {
            return true;
        }

        if (IsInRole(RoleNames.EducationExpert) &&
            _currentUser.UserProfileId.HasValue)
        {
            var majorId = await (
                from offering in _dbContext.CourseOfferings
                join course in _dbContext.Courses
                    on offering.CourseId equals course.Id
                where offering.Id == courseOfferingId &&
                      offering.IsActive
                select (Guid?)course.MajorId)
                .SingleOrDefaultAsync(cancellationToken);

            return majorId.HasValue &&
                   await _scopeService.HasMajorAccessAsync(
                       _currentUser.UserProfileId.Value,
                       majorId.Value,
                       cancellationToken);
        }

        if (IsInRole(RoleNames.Student))
        {
            var studentProfileId =
                await GetCurrentStudentProfileIdAsync(
                    cancellationToken);

            if (!studentProfileId.HasValue)
                return false;

            return await _dbContext.Enrollments.AnyAsync(
                x =>
                    x.CourseOfferingId == courseOfferingId &&
                    x.StudentProfileId ==
                    studentProfileId.Value &&
                    x.IsActive,
                cancellationToken);
        }

        return false;
    }

    public async Task<Guid?> GetCurrentStudentProfileIdAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.UserProfileId.HasValue)
            return null;

        return await _dbContext.StudentProfiles
            .Where(x =>
                x.UserProfileId ==
                _currentUser.UserProfileId.Value)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<Guid?> GetCurrentInstructorProfileIdAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.UserProfileId.HasValue)
            return null;

        return await _dbContext.InstructorProfiles
            .Where(x =>
                x.UserProfileId ==
                _currentUser.UserProfileId.Value)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private bool IsInRole(string role) =>
        _currentUser.Roles.Contains(
            role,
            StringComparer.OrdinalIgnoreCase);
}