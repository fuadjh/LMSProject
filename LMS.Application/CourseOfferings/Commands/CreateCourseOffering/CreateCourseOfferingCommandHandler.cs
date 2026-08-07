using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Abstractions.Security;
using Application.Common.Results;
using Common.Security;
using Domain.Entities.Academics;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.CourseOfferings.Commands.CreateCourseOffering;

public sealed class CreateCourseOfferingCommandHandler
    : IRequestHandler<CreateCourseOfferingCommand, Result<Guid>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IAccessScopeService _scopeService;
    private readonly IApplicationDbContext _dbContext;

    public CreateCourseOfferingCommandHandler(
        ICurrentUser currentUser,
        IAccessScopeService scopeService,
        IApplicationDbContext dbContext)
    {
        _currentUser = currentUser;
        _scopeService = scopeService;
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(
        CreateCourseOfferingCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result<Guid>.Unauthorized(
                "auth.required",
                "کاربر احراز هویت نشده است.");
        }

        var course = await _dbContext.Courses
            .Where(x =>
                x.Id == request.CourseId &&
                x.IsActive)
            .Select(x => new
            {
                x.Id,
                x.MajorId
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (course is null)
        {
            return Result<Guid>.NotFound(
                "course.not_found",
                "درس یافت نشد.");
        }

        var semester = await _dbContext.Semesters
            .Where(x =>
                x.Id == request.SemesterId &&
                x.IsActive)
            .Select(x => new
            {
                x.StartsAtUtc,
                x.EndsAtUtc
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (semester is null)
        {
            return Result<Guid>.NotFound(
                "semester.not_found",
                "نیم‌سال یافت نشد.");
        }

        var isAdmin = _currentUser.Roles.Contains(
            RoleNames.Admin,
            StringComparer.OrdinalIgnoreCase);

        if (!isAdmin)
        {
            if (!_currentUser.UserId.HasValue)
            {
                return Result<Guid>.Forbidden(
                    "scope.user_profile_required",
                    "پروفایل کاربر یافت نشد.");
            }

            var hasScope =
                await _scopeService.HasMajorAccessAsync(
                    _currentUser.UserId.Value,
                    course.MajorId,
                    cancellationToken);

            if (!hasScope)
            {
                return Result<Guid>.Forbidden(
                    "scope.denied",
                    "دسترسی به این رشته را ندارید.");
            }
        }

        var normalizedSectionCode =
            request.SectionCode.Trim().ToUpperInvariant();

        var exists = await _dbContext.CourseOfferings
            .AnyAsync(
                x =>
                    x.CourseId == request.CourseId &&
                    x.SemesterId == request.SemesterId &&
                    x.SectionCode == normalizedSectionCode,
                cancellationToken);

        if (exists)
        {
            return Result<Guid>.Conflict(
                "offering.section_exists",
                "این گروه برای درس و نیم‌سال انتخاب‌شده قبلاً ثبت شده است.");
        }

        var startsAtUtc =
            request.StartsAtUtc ?? semester.StartsAtUtc;

        var endsAtUtc =
            request.EndsAtUtc ?? semester.EndsAtUtc;

        try
        {
            var offering = CourseOffering.Create(
                request.CourseId,
                request.SemesterId,
                normalizedSectionCode,
                request.Capacity,
                startsAtUtc,
                endsAtUtc);

            await _dbContext.AddAsync(
                offering,
                cancellationToken);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return Result<Guid>.Success(offering.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Invalid(
                Error.Validation(
                    "offering.invalid_data",
                    ex.Message));
        }
    }
}