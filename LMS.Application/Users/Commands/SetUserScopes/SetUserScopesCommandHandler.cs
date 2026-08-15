using Application.Abstractions.Persistence;
using Application.Common.Results;
using Domain.Entities.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.SetUserScopes;

public sealed class SetUserScopesCommandHandler
    : IRequestHandler<
        SetUserScopesCommand,
        Result>
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
        var userExists = await _dbContext.StudentProfiles
            .AnyAsync(x => x.Id == request.UserId, cancellationToken)
            || await _dbContext.InstructorProfiles
                .AnyAsync(x => x.Id == request.UserId, cancellationToken)
            || await _dbContext.ExpertProfiles
                .AnyAsync(x => x.Id == request.UserId, cancellationToken);

        if (!userExists)
        {
            return Result.NotFound(
                "User.NotFound",
                "کاربر پیدا نشد.");
        }

        var facultyIds = request.FacultyIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();

        var majorIds = request.MajorIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();

        var validFacultyIds = await _dbContext.Faculties
            .Where(x => facultyIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (validFacultyIds.Count != facultyIds.Length)
        {
            return Result.Invalid(
                Error.Validation(
                    "Faculty.Invalid",
                    "یک یا چند دانشکده معتبر نیست."));
        }

        var validMajorIds = await _dbContext.Majors
            .Where(x => majorIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (validMajorIds.Count != majorIds.Length)
        {
            return Result.Invalid(
                Error.Validation(
                    "Major.Invalid",
                    "یک یا چند رشته معتبر نیست."));
        }

        var oldFacultyScopes = await _dbContext.UserFacultyScopes
            .Where(x => x.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        foreach (var item in oldFacultyScopes)
        {
            _dbContext.Remove(item);
        }

        var oldMajorScopes = await _dbContext.UserMajorScopes
            .Where(x => x.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        foreach (var item in oldMajorScopes)
        {
            _dbContext.Remove(item);
        }

        foreach (var facultyId in validFacultyIds)
        {
            await _dbContext.AddAsync(
                UserFacultyScope.Create(
                    request.UserId,
                    request.RoleType,
                    facultyId),
                cancellationToken);
        }

        foreach (var majorId in validMajorIds)
        {
            await _dbContext.AddAsync(
                UserMajorScope.Create(
                    request.UserId,
                    request.RoleType,
                    majorId),
                cancellationToken);
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}