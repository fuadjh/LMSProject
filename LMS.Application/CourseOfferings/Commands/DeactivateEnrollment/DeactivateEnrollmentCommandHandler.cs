using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Abstractions.Security;
using Application.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.CourseOfferings.Commands.DeactivateEnrollment;

public sealed class DeactivateEnrollmentCommandHandler : IRequestHandler<DeactivateEnrollmentCommand, Result>
{
    private readonly ICurrentUser _currentUser;
    private readonly IAccessScopeService _scopeService;
    private readonly IApplicationDbContext _dbContext;

    public DeactivateEnrollmentCommandHandler(
        ICurrentUser currentUser,
        IAccessScopeService scopeService,
        IApplicationDbContext dbContext)
    {
        _currentUser = currentUser;
        _scopeService = scopeService;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DeactivateEnrollmentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            return Result.Unauthorized("auth.required", "کاربر احراز هویت نشده است.");

        var data = await (
            from e in _dbContext.Enrollments
            join o in _dbContext.CourseOfferings on e.CourseOfferingId equals o.Id
            join c in _dbContext.Courses on o.CourseId equals c.Id
            where e.Id == request.EnrollmentId
            select new { Enrollment = e, c.MajorId })
            .SingleOrDefaultAsync(cancellationToken);

        if (data is null)
            return Result.NotFound("enrollment.not_found", "ثبت‌نام یافت نشد.");

        var hasScope = await _scopeService.HasMajorAccessAsync(_currentUser.UserId.Value, data.MajorId, cancellationToken);
        if (!hasScope)
            return Result.Forbidden("scope.denied", "دسترسی به این رشته را ندارید.");

        data.Enrollment.Deactivate();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}