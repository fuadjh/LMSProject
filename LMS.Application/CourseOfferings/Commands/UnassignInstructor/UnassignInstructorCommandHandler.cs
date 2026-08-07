using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Abstractions.Security;
using Application.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.CourseOfferings.Commands.UnassignInstructor;

public sealed class UnassignInstructorCommandHandler : IRequestHandler<UnassignInstructorCommand, Result>
{
    private readonly ICurrentUser _currentUser;
    private readonly IAccessScopeService _scopeService;
    private readonly IApplicationDbContext _dbContext;

    public UnassignInstructorCommandHandler(
        ICurrentUser currentUser,
        IAccessScopeService scopeService,
        IApplicationDbContext dbContext)
    {
        _currentUser = currentUser;
        _scopeService = scopeService;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(UnassignInstructorCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            return Result.Unauthorized("auth.required", "کاربر احراز هویت نشده است.");

        var data = await (
            from o in _dbContext.CourseOfferings
            join c in _dbContext.Courses on o.CourseId equals c.Id
            where o.Id == request.CourseOfferingId
            select new { Offering = o, c.MajorId })
            .SingleOrDefaultAsync(cancellationToken);

        if (data is null)
            return Result.NotFound("offering.not_found", "ارائه یافت نشد.");

        var hasScope = await _scopeService.HasMajorAccessAsync(_currentUser.UserId.Value, data.MajorId, cancellationToken);
        if (!hasScope)
            return Result.Forbidden("scope.denied", "دسترسی به این رشته را ندارید.");

        data.Offering.UnassignInstructor();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}