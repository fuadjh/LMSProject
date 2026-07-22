using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Abstractions.Security;
using Application.Common.Results;
using Domain.Entities.Academics;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Courses.Commands.CreateCourse;

public sealed class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, Result<Guid>>
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

    public async Task<Result<Guid>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserProfileId.HasValue)
            return Result<Guid>.Unauthorized("auth.required", "کاربر احراز هویت نشده است.");

        var major = await _dbContext.Majors
            .Where(x => x.Id == request.MajorId && x.IsActive)
            .Select(x => new { x.Id, x.FacultyId })
            .SingleOrDefaultAsync(cancellationToken);

        if (major is null)
            return Result<Guid>.NotFound("major.not_found", "رشته یافت نشد.");

        var hasScope = await _scopeService.HasMajorAccessAsync(
            _currentUser.UserProfileId.Value,
            request.MajorId,
            cancellationToken);

        //if (!hasScope)
        //    return Result<Guid>.Forbidden("scope.denied", "دسترسی به این رشته را ندارید.");

        var exists = await _dbContext.Courses.AnyAsync(
            x => x.MajorId == request.MajorId && x.Code == request.Code,
            cancellationToken);

        if (exists)
            return Result<Guid>.Conflict("course.code_exists", "کد درس تکراری است.");

        var course = Course.Create(request.MajorId, request.Title, request.Code, request.Units);

        await _dbContext.AddAsync(course, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(course.Id);
    }
}