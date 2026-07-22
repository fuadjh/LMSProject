using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Abstractions.Security;
using Application.Common.Results;
using Common.Security;
using Domain.Entities.Academics;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Courses.Commands.CreateCourse;

public sealed class CreateCourseCommandHandler
    : IRequestHandler<CreateCourseCommand, Result<Guid>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IAccessScopeService _scopeService;
    private readonly IApplicationDbContext _dbContext;

    public CreateCourseCommandHandler(
        ICurrentUser currentUser,
        IAccessScopeService scopeService,
        IApplicationDbContext dbContext)
    {
        _currentUser = currentUser;
        _scopeService = scopeService;
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(
        CreateCourseCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result<Guid>.Unauthorized(
                "auth.required",
                "کاربر احراز هویت نشده است.");
        }

        var majorExists = await _dbContext.Majors.AnyAsync(
            x => x.Id == request.MajorId && x.IsActive,
            cancellationToken);

        if (!majorExists)
        {
            return Result<Guid>.NotFound(
                "major.not_found",
                "رشته یافت نشد یا غیرفعال است.");
        }

        var isAdmin = _currentUser.Roles.Contains(
            RoleNames.Admin,
            StringComparer.OrdinalIgnoreCase);

        if (!isAdmin)
        {
            if (!_currentUser.UserProfileId.HasValue)
            {
                return Result<Guid>.Forbidden(
                    "profile.required",
                    "پروفایل کاربر یافت نشد.");
            }

            var hasScope =
                await _scopeService.HasMajorAccessAsync(
                    _currentUser.UserProfileId.Value,
                    request.MajorId,
                    cancellationToken);

            if (!hasScope)
            {
                return Result<Guid>.Forbidden(
                    "scope.denied",
                    "دسترسی به این رشته را ندارید.");
            }
        }

        var normalizedCode =
            request.Code.Trim().ToUpperInvariant();

        var duplicateExists =
            await _dbContext.Courses.AnyAsync(
                x => x.MajorId == request.MajorId &&
                     x.Code == normalizedCode,
                cancellationToken);

        if (duplicateExists)
        {
            return Result<Guid>.Conflict(
                "course.code_exists",
                "کد درس در رشته انتخاب‌شده تکراری است.");
        }

        var course = Course.Create(
            request.MajorId,
            request.Title,
            normalizedCode,
            request.Units,
            request.EnrollmentScope);

        await _dbContext.AddAsync(
            course,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result<Guid>.Success(course.Id);
    }
}