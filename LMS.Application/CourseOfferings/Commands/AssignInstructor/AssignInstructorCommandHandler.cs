using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Abstractions.Security;
using Application.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.CourseOfferings.Commands.AssignInstructor;

public sealed class AssignInstructorCommandHandler : IRequestHandler<AssignInstructorCommand, Result>
{
    private readonly ICurrentUser _currentUser;
    private readonly IAccessScopeService _scopeService;
    private readonly IApplicationDbContext _dbContext;

    public AssignInstructorCommandHandler(
        ICurrentUser currentUser,
        IAccessScopeService scopeService,
        IApplicationDbContext dbContext)
    {
        _currentUser = currentUser;
        _scopeService = scopeService;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(AssignInstructorCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserProfileId.HasValue)
            return Result.Unauthorized("auth.required", "کاربر احراز هویت نشده است.");

        var offeringData = await (
            from o in _dbContext.CourseOfferings
            join c in _dbContext.Courses on o.CourseId equals c.Id
            where o.Id == request.CourseOfferingId && o.IsActive
            select new { Offering = o, c.MajorId })
            .SingleOrDefaultAsync(cancellationToken);

        if (offeringData is null)
            return Result.NotFound("offering.not_found", "ارائه درس یافت نشد.");

        var instructorExists = await _dbContext.InstructorProfiles
            .AnyAsync(x => x.Id == request.InstructorProfileId, cancellationToken);

        if (!instructorExists)
            return Result.NotFound("instructor.not_found", "استاد یافت نشد.");

        var hasScope = await _scopeService.HasMajorAccessAsync(
            _currentUser.UserProfileId.Value,
            offeringData.MajorId,
            cancellationToken);

        if (!hasScope)
            return Result.Forbidden("scope.denied", "دسترسی به این رشته را ندارید.");

        offeringData.Offering.AssignInstructor(request.InstructorProfileId);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}