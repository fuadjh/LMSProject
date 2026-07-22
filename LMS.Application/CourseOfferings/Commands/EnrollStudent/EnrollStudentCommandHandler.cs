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

        var data = await (
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

        if (data is null)
        {
            return Result.NotFound(
                "offering.not_found",
                "گروه درسی یافت نشد یا غیرفعال است.");
        }

        var isAdmin = _currentUser.Roles.Contains(
            RoleNames.Admin,
            StringComparer.OrdinalIgnoreCase);

        if (!isAdmin)
        {
            if (!_currentUser.UserProfileId.HasValue)
            {
                return Result.Forbidden(
                    "profile.required",
                    "پروفایل کاربر یافت نشد.");
            }

            var hasScope =
                await _scopeService.HasMajorAccessAsync(
                    _currentUser.UserProfileId.Value,
                    data.Course.MajorId,
                    cancellationToken);

            if (!hasScope)
            {
                return Result.Forbidden(
                    "scope.denied",
                    "دسترسی به رشته مالک درس را ندارید.");
            }
        }

        var student = await (
                from studentProfile in _dbContext.StudentProfiles
                join userProfile in _dbContext.UserProfiles
                    on studentProfile.UserProfileId equals userProfile.Id
                where studentProfile.Id == request.StudentProfileId &&
                      userProfile.IsActive
                select studentProfile)
            .SingleOrDefaultAsync(cancellationToken);

        if (student is null)
        {
            return Result.NotFound(
                "student.not_found",
                "دانشجو یافت نشد یا غیرفعال است.");
        }

        if (!data.Course.AllowsEnrollmentFor(student.MajorId))
        {
            return Result.Forbidden(
                "student.major_not_allowed",
                "رشته دانشجو مجاز به ثبت‌نام در این درس نیست.");
        }

        var existingEnrollment =
            await _dbContext.Enrollments.SingleOrDefaultAsync(
                x => x.CourseOfferingId == request.CourseOfferingId &&
                     x.StudentProfileId == request.StudentProfileId,
                cancellationToken);

        if (existingEnrollment?.IsActive == true)
        {
            return Result.Conflict(
                "enrollment.exists",
                "این دانشجو قبلاً در گروه ثبت‌نام شده است.");
        }

        var activeEnrollmentCount =
            await _dbContext.Enrollments.CountAsync(
                x => x.CourseOfferingId == request.CourseOfferingId &&
                     x.IsActive,
                cancellationToken);

        if (!data.Offering.HasAvailableCapacity(
                activeEnrollmentCount))
        {
            return Result.Conflict(
                "offering.capacity_full",
                "ظرفیت گروه درسی تکمیل شده است.");
        }

        if (existingEnrollment is null)
        {
            var enrollment = Enrollment.Create(
                request.CourseOfferingId,
                request.StudentProfileId);

            await _dbContext.AddAsync(
                enrollment,
                cancellationToken);
        }
        else
        {
            existingEnrollment.Activate();
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}