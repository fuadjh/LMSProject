using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Abstractions.Security;
using Application.Common.Results;
using Common.Security;
using Domain.Entities.Academics;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.CourseOfferings.Commands.EnrollStudent;

public sealed class EnrollStudentCommandHandler
    : IRequestHandler<EnrollStudentCommand, Result>
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

    public async Task<Result> Handle(
        EnrollStudentCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result.Unauthorized(
                "auth.required",
                "کاربر احراز هویت نشده است.");
        }

        var offeringData = await (
            from offering in _dbContext.CourseOfferings
            join course in _dbContext.Courses
                on offering.CourseId equals course.Id
            where offering.Id == request.CourseOfferingId &&
                  offering.IsActive &&
                  course.IsActive
            select new
            {
                Offering = offering,
                Course = course
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (offeringData is null)
        {
            return Result.NotFound(
                "offering.not_found",
                "ارائه درس یافت نشد.");
        }

        var isAdmin = _currentUser.Roles.Contains(
            RoleNames.Admin,
            StringComparer.OrdinalIgnoreCase);

        if (!isAdmin)
        {
            if (!_currentUser.UserId.HasValue)
            {
                return Result.Forbidden(
                    "scope.user_profile_required",
                    "پروفایل کاربر یافت نشد.");
            }

            var hasScope =
                await _scopeService.HasMajorAccessAsync(
                    _currentUser.UserId.Value,
                    offeringData.Course.MajorId,
                    cancellationToken);

            if (!hasScope)
            {
                return Result.Forbidden(
                    "scope.denied",
                    "دسترسی به این رشته را ندارید.");
            }
        }

        var student = await _dbContext.StudentProfiles
            .Where(x => x.Id == request.StudentProfileId)
            .Select(x => new
            {
                x.Id,
                x.MajorId
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (student is null)
        {
            return Result.NotFound(
                "student.not_found",
                "دانشجو یافت نشد.");
        }

        if (!offeringData.Course.CanEnrollMajor(
                student.MajorId))
        {
            return Result.Forbidden(
                "student.major_mismatch",
                "این درس فقط برای دانشجویان رشته مالک قابل ثبت‌نام است.");
        }

        var activeEnrollmentCount =
            await _dbContext.Enrollments.CountAsync(
                x =>
                    x.CourseOfferingId ==
                    request.CourseOfferingId &&
                    x.IsActive,
                cancellationToken);

        if (!offeringData.Offering.HasCapacity(
                activeEnrollmentCount))
        {
            return Result.Conflict(
                "offering.capacity_full",
                "ظرفیت گروه تکمیل شده است.");
        }

        var existingEnrollment =
            await _dbContext.Enrollments
                .SingleOrDefaultAsync(
                    x =>
                        x.CourseOfferingId ==
                        request.CourseOfferingId &&
                        x.StudentProfileId ==
                        request.StudentProfileId,
                    cancellationToken);

        if (existingEnrollment is not null)
        {
            if (existingEnrollment.IsActive)
            {
                return Result.Conflict(
                    "enrollment.exists",
                    "دانشجو قبلاً در این گروه ثبت‌نام شده است.");
            }

            existingEnrollment.Activate();

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return Result.Success();
        }

        var enrollment = Enrollment.Create(
            request.CourseOfferingId,
            request.StudentProfileId);

        await _dbContext.AddAsync(
            enrollment,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}