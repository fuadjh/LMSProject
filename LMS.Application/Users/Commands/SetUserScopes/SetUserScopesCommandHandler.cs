using Application.Abstractions.Persistence;
using Application.Common.Results;
using Common.Enums;
using Domain.Entities.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.SetUserScopes;

public sealed class SetUserScopesCommandHandler
    : IRequestHandler<SetUserScopesCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;

    public SetUserScopesCommandHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(
        SetUserScopesCommand request,
        CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            return Result.Invalid(
                "userId",
                "شناسه کاربر الزامی است.");
        }

        if (!Enum.IsDefined(request.RoleType))
        {
            return Result.Invalid(
                "roleType",
                "نوع نقش کاربر معتبر نیست.");
        }

        var roleExists =
            await RoleExistsAsync(
                request.UserId,
                request.RoleType,
                cancellationToken);

        if (!roleExists)
        {
            return Result.NotFound(
                "user.role_not_found",
                "نقش مورد نظر برای این کاربر یافت نشد.");
        }

        var facultyIds =
            (request.FacultyIds ?? Array.Empty<Guid>())
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        var majorIds =
            (request.MajorIds ?? Array.Empty<Guid>())
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        var validFacultyIds =
            await _dbContext.Faculties
                .AsNoTracking()
                .Where(faculty =>
                    facultyIds.Contains(faculty.Id) &&
                    faculty.IsActive)
                .Select(faculty => faculty.Id)
                .ToArrayAsync(cancellationToken);

        if (facultyIds.Except(validFacultyIds).Any())
        {
            return Result.Invalid(
                "facultyIds",
                "یک یا چند دانشکده معتبر یا فعال نیست.");
        }

        var validMajors =
            await _dbContext.Majors
                .AsNoTracking()
                .Where(major =>
                    majorIds.Contains(major.Id) &&
                    major.IsActive)
                .Select(major => new
                {
                    major.Id,
                    major.FacultyId
                })
                .ToArrayAsync(cancellationToken);

        if (majorIds
            .Except(validMajors.Select(major => major.Id))
            .Any())
        {
            return Result.Invalid(
                "majorIds",
                "یک یا چند رشته معتبر یا فعال نیست.");
        }

        var invalidMajorFaculty =
            validMajors.Any(major =>
                !facultyIds.Contains(major.FacultyId));

        if (invalidMajorFaculty)
        {
            return Result.Invalid(
                "majorIds",
                "دانشکده مربوط به رشته‌های انتخاب‌شده در محدوده دانشکده‌های کاربر نیست.");
        }

        var oldFacultyScopes =
            await _dbContext.UserFacultyScopes
                .Where(scope =>
                    scope.UserId == request.UserId &&
                    scope.RoleType == request.RoleType)
                .ToListAsync(cancellationToken);

        var oldMajorScopes =
            await _dbContext.UserMajorScopes
                .Where(scope =>
                    scope.UserId == request.UserId &&
                    scope.RoleType == request.RoleType)
                .ToListAsync(cancellationToken);

        foreach (var scope in oldFacultyScopes)
        {
            _dbContext.Remove(scope);
        }

        foreach (var scope in oldMajorScopes)
        {
            _dbContext.Remove(scope);
        }

        foreach (var facultyId in validFacultyIds)
        {
            var scope =
                UserFacultyScope.Create(
                    request.UserId,
                    request.RoleType,
                    facultyId);

            await _dbContext.AddAsync(
                scope,
                cancellationToken);
        }

        foreach (var major in validMajors)
        {
            var scope =
                UserMajorScope.Create(
                    request.UserId,
                    request.RoleType,
                    major.Id);

            await _dbContext.AddAsync(
                scope,
                cancellationToken);
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }

    private Task<bool> RoleExistsAsync(
        Guid userId,
        UserRoleType roleType,
        CancellationToken cancellationToken)
    {
        return roleType switch
        {
            UserRoleType.Student =>
                _dbContext.StudentProfiles
                    .AsNoTracking()
                    .AnyAsync(
                        profile => profile.Id == userId,
                        cancellationToken),

            UserRoleType.Instructor =>
                _dbContext.InstructorProfiles
                    .AsNoTracking()
                    .AnyAsync(
                        profile => profile.Id == userId,
                        cancellationToken),

            UserRoleType.Expert =>
                _dbContext.ExpertProfiles
                    .AsNoTracking()
                    .AnyAsync(
                        profile => profile.Id == userId,
                        cancellationToken),

            _ => Task.FromResult(false)
        };
    }
}