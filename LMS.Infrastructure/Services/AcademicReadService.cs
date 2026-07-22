using Application.Abstractions.Read;
using Application.Common.Models;
using Common.Contracts.Academic;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class AcademicReadService : IAcademicReadService
{
    private readonly LmsDbContext _dbContext;

    public AcademicReadService(LmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<CourseLookupDto>> GetCoursesAsync(Guid? majorId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Courses.Where(x => x.IsActive);

        if (majorId.HasValue)
            query = query.Where(x => x.MajorId == majorId.Value);

        return await query
            .OrderBy(x => x.Title)
            .Select(x => new CourseLookupDto(x.Id, x.Title, x.Code, x.Units, x.MajorId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<SemesterLookupDto>> GetSemestersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Semesters
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.StartsAtUtc)
            .Select(x => new SemesterLookupDto(x.Id, x.Title, x.StartsAtUtc, x.EndsAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<CourseOfferingLookupDto>> GetCourseOfferingsAsync(Guid? semesterId, CancellationToken cancellationToken = default)
    {
        var query =
            from o in _dbContext.CourseOfferings
            join c in _dbContext.Courses on o.CourseId equals c.Id
            join s in _dbContext.Semesters on o.SemesterId equals s.Id
            join i in _dbContext.InstructorProfiles on o.InstructorProfileId equals i.Id into instructorJoin
            from instructor in instructorJoin.DefaultIfEmpty()
            join up in _dbContext.UserProfiles on instructor.UserProfileId equals up.Id into userProfileJoin
            from profile in userProfileJoin.DefaultIfEmpty()
            where o.IsActive
            select new CourseOfferingLookupDto(
                o.Id,
                c.Id,
                s.Id,
                c.MajorId,
                c.Title,
                s.Title,
                profile == null ? null : profile.FirstName + " " + profile.LastName,
                o.InstructorProfileId);

        if (semesterId.HasValue)
            query = query.Where(x => x.SemesterId == semesterId.Value);

        return await query
            .OrderBy(x => x.SemesterTitle)
            .ThenBy(x => x.CourseTitle)
            .ToListAsync(cancellationToken);
    }

    public async Task<CourseOfferingDetailsDto?> GetCourseOfferingAsync(Guid offeringId, CancellationToken cancellationToken = default)
    {
        var data =
            await (
                from o in _dbContext.CourseOfferings
                join c in _dbContext.Courses on o.CourseId equals c.Id
                join s in _dbContext.Semesters on o.SemesterId equals s.Id
                join i in _dbContext.InstructorProfiles on o.InstructorProfileId equals i.Id into instructorJoin
                from instructor in instructorJoin.DefaultIfEmpty()
                join up in _dbContext.UserProfiles on instructor.UserProfileId equals up.Id into userProfileJoin
                from profile in userProfileJoin.DefaultIfEmpty()
                where o.Id == offeringId
                select new CourseOfferingDetailsDto(
                    o.Id,
                    c.Id,
                    s.Id,
                    c.MajorId,
                    c.Title,
                    s.Title,
                    o.InstructorProfileId,
                    profile == null ? null : profile.FirstName + " " + profile.LastName
                ))
            .SingleOrDefaultAsync(cancellationToken);

        return data;
    }

    public async Task<IReadOnlyCollection<InstructorLookupDto>> SearchInstructorsAsync(string? search, CancellationToken cancellationToken = default)
    {
        var query =
            from i in _dbContext.InstructorProfiles
            join p in _dbContext.UserProfiles on i.UserProfileId equals p.Id
            select new { i, p };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(x =>
                x.i.PersonnelCode.Contains(s) ||
                (x.p.FirstName + " " + x.p.LastName).Contains(s));
        }

        return await query
            .OrderBy(x => x.p.FirstName)
            .ThenBy(x => x.p.LastName)
            .Take(50)
            .Select(x => new InstructorLookupDto(
                x.i.Id,
                x.p.Id,
                x.p.FirstName + " " + x.p.LastName,
                x.i.PersonnelCode))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<StudentLookupDto>> SearchStudentsAsync(Guid? majorId, string? search, CancellationToken cancellationToken = default)
    {
        var query =
            from s in _dbContext.StudentProfiles
            join p in _dbContext.UserProfiles on s.UserProfileId equals p.Id
            select new { s, p };

        if (majorId.HasValue)
            query = query.Where(x => x.s.MajorId == majorId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var text = search.Trim();
            query = query.Where(x =>
                x.s.StudentNumber.Contains(text) ||
                (x.p.FirstName + " " + x.p.LastName).Contains(text));
        }

        return await query
            .OrderBy(x => x.p.FirstName)
            .ThenBy(x => x.p.LastName)
            .Take(100)
            .Select(x => new StudentLookupDto(
                x.s.Id,
                x.p.Id,
                x.p.FirstName + " " + x.p.LastName,
                x.s.StudentNumber,
                x.s.MajorId))
            .ToListAsync(cancellationToken);
    }
}