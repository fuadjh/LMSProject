using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Abstractions.Security;
using Application.Common.Results;
using Common.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Courses.Commands.UpdateCourse;

public sealed class UpdateCourseCommandHandler
    : IRequestHandler<UpdateCourseCommand, Result>
{
    private readonly ICurrentUser _currentUser;
    private readonly IAccessScopeService _scopeService;
    private readonly IApplicationDbContext _dbContext;

    public UpdateCourseCommandHandler(
        ICurrentUser currentUser,
        IAccessScopeService scopeService,
        IApplicationDbContext dbContext)
    {
        _currentUser = currentUser;
        _scopeService = scopeService;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(
        UpdateCourseCommand request,
        CancellationToken cancellationToken)
    {
        var course = await _dbContext.Courses
            .SingleOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (course is null)
        {
            return Result.NotFound(
                "course.not_found",
                "درس یافت نشد.");
        }

        var targetMajorExists =
            await _dbContext.Majors.AnyAsync(
                x => x.Id == request.MajorId &&
                     x.IsActive,
                cancellationToken);

        if (!targetMajorExists)
        {
            return Result.NotFound(
                "major.not_found",
                "رشته مقصد یافت نشد.");
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

            var currentAccess =
                await _scopeService.HasMajorAccessAsync(
                    _currentUser.UserProfileId.Value,
                    course.MajorId,
                    cancellationToken);

            var targetAccess =
                course.MajorId == request.MajorId ||
                await _scopeService.HasMajorAccessAsync(
                    _currentUser.UserProfileId.Value,
                    request.MajorId,
                    cancellationToken);

            if (!currentAccess || !targetAccess)
            {
                return Result.Forbidden(
                    "scope.denied",
                    "دسترسی لازم برای ویرایش درس را ندارید.");
            }
        }

        if (course.MajorId != request.MajorId)
        {
            var hasOfferings =
                await _dbContext.CourseOfferings.AnyAsync(
                    x => x.CourseId == course.Id,
                    cancellationToken);

            if (hasOfferings)
            {
                return Result.Conflict(
                    "course.has_offerings",
                    "درس دارای ارائه قابل انتقال به رشته دیگر نیست.");
            }
        }

        var normalizedCode =
            request.Code.Trim().ToUpperInvariant();

        var duplicateExists =
            await _dbContext.Courses.AnyAsync(
                x => x.Id != request.Id &&
                     x.MajorId == request.MajorId &&
                     x.Code == normalizedCode,
                cancellationToken);

        if (duplicateExists)
        {
            return Result.Conflict(
                "course.code_exists",
                "کد درس تکراری است.");
        }

        course.Update(
            request.MajorId,
            request.Title,
            normalizedCode,
            request.Units,
            request.EnrollmentScope);

        if (request.IsActive)
            course.Activate();
        else
            course.Deactivate();

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}