using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Abstractions.Security;
using Application.Common.Results;
using Domain.Entities.Academics;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LMS.Application.CourseOfferings.Commands.CreateCourseOffering;

public sealed class CreateCourseOfferingCommandHandler : IRequestHandler<CreateCourseOfferingCommand, Result<Guid>>
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

    public async Task<Result<Guid>> Handle(CreateCourseOfferingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserProfileId.HasValue)
            return Result<Guid>.Unauthorized("auth.required", "کاربر احراز هویت نشده است.");

        var course = await _dbContext.Courses
            .Where(x => x.Id == request.CourseId && x.IsActive)
            .Select(x => new { x.Id, x.MajorId })
            .SingleOrDefaultAsync(cancellationToken);

        if (course is null)
            return Result<Guid>.NotFound("course.not_found", "درس یافت نشد.");

        var semesterExists = await _dbContext.Semesters
            .AnyAsync(x => x.Id == request.SemesterId && x.IsActive, cancellationToken);

        if (!semesterExists)
            return Result<Guid>.NotFound("semester.not_found", "نیمسال یافت نشد.");

        var hasScope = await _scopeService.HasMajorAccessAsync(
            _currentUser.UserProfileId.Value,
            course.MajorId,
            cancellationToken);

        if (!hasScope)
            return Result<Guid>.Forbidden("scope.denied", "دسترسی به این رشته را ندارید.");

        var exists = await _dbContext.CourseOfferings
            .AnyAsync(x => x.CourseId == request.CourseId && x.SemesterId == request.SemesterId, cancellationToken);

        if (exists)
            return Result<Guid>.Conflict("offering.exists", "این ارائه قبلاً ثبت شده است.");

        var offering = CourseOffering.Create(request.CourseId, request.SemesterId);

        await _dbContext.AddAsync(offering, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(offering.Id);
    }
}