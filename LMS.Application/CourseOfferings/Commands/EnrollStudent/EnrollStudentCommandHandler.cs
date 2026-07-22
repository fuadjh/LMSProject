using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Abstractions.Security;
using Application.Common.Results;
using Domain.Entities.Academics;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.CourseOfferings.Commands.EnrollStudent;

public sealed class EnrollStudentCommandHandler : IRequestHandler<EnrollStudentCommand, Result>
{
    private readonly ICurrentUser _currentUser;
    private readonly IAccessScopeService _scopeService;
    private readonly IApplicationDbContext _dbContext;

    public EnrollStudentCommandHandler(
        ICurrentUser currentUser,
        IAccessScopeService scopeService,
        IApplicationDbContext dbContext)
    {
        _currentUser = currentUser;
        _scopeService = scopeService;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(EnrollStudentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserProfileId.HasValue)
            return Result.Unauthorized("auth.required", "کاربر احراز هویت نشده است.");

        var offeringData = await (
            from o in _dbContext.CourseOfferings
            join c in _dbContext.Courses on o.CourseId equals c.Id
            where o.Id == request.CourseOfferingId && o.IsActive
            select new { o.Id, c.MajorId })
            .SingleOrDefaultAsync(cancellationToken);

        if (offeringData is null)
            return Result.NotFound("offering.not_found", "ارائه درس یافت نشد.");

        var hasScope = await _scopeService.HasMajorAccessAsync(
            _currentUser.UserProfileId.Value,
            offeringData.MajorId,
            cancellationToken);

        if (!hasScope)
            return Result.Forbidden("scope.denied", "دسترسی به این رشته را ندارید.");

        var student = await _dbContext.StudentProfiles
            .Where(x => x.Id == request.StudentProfileId)
            .Select(x => new { x.Id, x.MajorId })
            .SingleOrDefaultAsync(cancellationToken);

        if (student is null)
            return Result.NotFound("student.not_found", "دانشجو یافت نشد.");

        if (student.MajorId != offeringData.MajorId)
            return Result.Forbidden("student.major_mismatch", "دانشجو متعلق به رشته این درس نیست.");

        var exists = await _dbContext.Enrollments
            .AnyAsync(x =>
                x.CourseOfferingId == request.CourseOfferingId &&
                x.StudentProfileId == request.StudentProfileId &&
                x.IsActive,
                cancellationToken);

        if (exists)
            return Result.Conflict("enrollment.exists", "این دانشجو قبلاً در این ارائه ثبت شده است.");

        var enrollment = Enrollment.Create(request.CourseOfferingId, request.StudentProfileId);

        await _dbContext.AddAsync(enrollment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}