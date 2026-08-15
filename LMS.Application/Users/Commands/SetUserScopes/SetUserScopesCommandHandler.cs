using Application.Abstractions.Persistence;
using Application.Common.Results;
using Domain.Entities.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.SetUserScopes;

public sealed class SetUserScopesCommandHandler
    : IRequestHandler<SetUserScopesCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public SetUserScopesCommandHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(
        SetUserScopesCommand request,
        CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            return Result<bool>.Invalid(
                "userId",
                "شناسه کاربر الزامی است.");
        }

        if (!Enum.IsDefined(request.RoleType))
        {
            return Result<bool>.Invalid(
                "roleType",
                "نوع نقش کاربر معتبر نیست.");
        }

        var facultyIds = (request.FacultyIds ??
                          Array.Empty<Guid>())
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        var majorIds = (request.MajorIds ??
                        Array.Empty<Guid>())
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        var userExists =
            await _dbContext.UserProfiles
                .AsNoTracking()
                .AnyAsync(
                    profile => profile.Id == request.UserId,
                    cancellationToken);

        if (!userExists)
        {
            return Result<bool>.NotFound(
                "user.not_found",
                "کاربر مورد نظر یافت نشد.");
        }

        if (facultyIds.Length > 0)
        {
            var validFacultyIds =
                await _dbContext.Faculties
                    .AsNoTracking()
                    .Where(faculty =>
                        facultyIds.Contains(faculty.Id) &&
                        faculty.IsActive)
                    .Select(faculty => faculty.Id)
                    .ToArrayAsync(cancellationToken);

            var invalidFacultyIds =
                facultyIds.Except(validFacultyIds).ToArray();

            if (invalidFacultyIds.Length > 0)
            {
                return Result<bool>.Invalid(
                    "facultyIds",
                    "یک یا چند دانشکده انتخاب‌شده معتبر یا فعال نیست.");
            }
        }

        if (majorIds.Length > 0)
        {
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

            var invalidMajorIds =
                majorIds
                    .Except(validMajors.Select(major => major.Id))
                    .ToArray();

            if (invalidMajorIds.Length > 0)
            {
                return Result<bool>.Invalid(
                    "majorIds",
                    "یک یا چند رشته انتخاب‌شده معتبر یا فعال نیست.");
            }

            var hasFacultyMismatch =
                validMajors.Any(major =>
                    !facultyIds.Contains(major.FacultyId));

            if (hasFacultyMismatch)
            {
                return Result<bool>.Invalid(
                    "majorIds",
                    "دانشکده مربوط به تمام رشته‌های انتخاب‌شده باید در محدوده دانشکده‌های کاربر قرار داشته باشد.");
            }
        }

        var existingFacultyScopes =
            await _dbContext.UserFacultyScopes
                .Where(scope =>
                    scope.UserId == request.UserId &&
                    scope.RoleType == request.RoleType)
                .ToListAsync(cancellationToken);

        var existingMajorScopes =
            await _dbContext.UserMajorScopes
                .Where(scope =>
                    scope.UserId == request.UserId &&
                    scope.RoleType == request.RoleType)
                .ToListAsync(cancellationToken);

        foreach (var facultyScope in existingFacultyScopes)
        {
            _dbContext.Remove(facultyScope);
        }

        foreach (var majorScope in existingMajorScopes)
        {
            _dbContext.Remove(majorScope);
        }

        foreach (var facultyId in facultyIds)
        {
            var facultyScope =
                UserFacultyScope.Create(
                    request.UserId,
                    request.RoleType,
                    facultyId);

            await _dbContext.AddAsync(
                facultyScope,
                cancellationToken);
        }

        foreach (var majorId in majorIds)
        {
            var majorScope =
                UserMajorScope.Create(
                    request.UserId,
                    request.RoleType,
                    majorId);

            await _dbContext.AddAsync(
                majorScope,
                cancellationToken);
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result<bool>.Success(true);
    }
}