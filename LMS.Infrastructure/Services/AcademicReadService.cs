using Application.Abstractions.Read;
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

    public async Task<IReadOnlyCollection<CourseLookupDto>> GetCoursesAsync(
        Guid? majorId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Courses
            .AsNoTracking()
            .Where(x => x.IsActive);

        if (majorId.HasValue)
        {
            query = query.Where(x => x.MajorId == majorId.Value);
        }

        return await query
            .OrderBy(x => x.Title)
            .Select(x => new CourseLookupDto(
                x.Id,
                x.Title,
                x.Code,
                x.Units,
                x.MajorId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<SemesterLookupDto>> GetSemestersAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Semesters
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.StartsAtUtc)
            .Select(x => new SemesterLookupDto(
                x.Id,
                x.Title,
                x.StartsAtUtc,
                x.EndsAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<CourseOfferingLookupDto>> GetCourseOfferingsAsync(
        Guid? semesterId,
        CancellationToken cancellationToken = default)
    {
        var query =
            from offering in _dbContext.CourseOfferings.AsNoTracking()

            join course in _dbContext.Courses.AsNoTracking()
                on offering.CourseId equals course.Id

            join semester in _dbContext.Semesters.AsNoTracking()
                on offering.SemesterId equals semester.Id

            join instructor in _dbContext.InstructorProfiles.AsNoTracking()
                on offering.InstructorProfileId equals instructor.Id
                into instructorJoin

            from instructor in instructorJoin.DefaultIfEmpty()

            join profile in _dbIUserAccountService.AsNoTracking()
                on instructor.UserId equals profile.Id
                into profileJoin

            from profile in profileJoin.DefaultIfEmpty()

            where offering.IsActive

            select new CourseOfferingLookupDto(
                offering.Id,
                course.Id,
                semester.Id,
                course.MajorId,
                course.Title,
                semester.Title,
                profile == null
                    ? null
                    : profile.FirstName + " " + profile.LastName,
                offering.InstructorProfileId,
                offering.SectionCode,
                offering.Capacity,
                offering.StartsAtUtc,
                offering.EndsAtUtc,
                offering.IsActive);

        if (semesterId.HasValue)
        {
            query = query.Where(
                x => x.SemesterId == semesterId.Value);
        }

        return await query
            .OrderBy(x => x.SemesterTitle)
            .ThenBy(x => x.CourseTitle)
            .ThenBy(x => x.SectionCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<CourseOfferingDetailsDto?> GetCourseOfferingAsync(
        Guid offeringId,
        CancellationToken cancellationToken = default)
    {
        var data =
            await (
                from offering in _dbContext.CourseOfferings.AsNoTracking()

                join course in _dbContext.Courses.AsNoTracking()
                    on offering.CourseId equals course.Id

                join semester in _dbContext.Semesters.AsNoTracking()
                    on offering.SemesterId equals semester.Id

                join instructor in _dbContext.InstructorProfiles.AsNoTracking()
                    on offering.InstructorProfileId equals instructor.Id
                    into instructorJoin

                from instructor in instructorJoin.DefaultIfEmpty()

                join profile in _dbIUserAccountService.AsNoTracking()
                    on instructor.UserId equals profile.Id
                    into profileJoin

                from profile in profileJoin.DefaultIfEmpty()

                where offering.Id == offeringId

                select new CourseOfferingDetailsDto(
                    offering.Id,
                    course.Id,
                    semester.Id,
                    course.MajorId,
                    course.Title,
                    course.Code,
                    course.Units,
                    semester.Title,
                    offering.InstructorProfileId,
                    profile == null
                        ? null
                        : profile.FirstName + " " + profile.LastName,
                    offering.SectionCode,
                    offering.Capacity,
                    offering.StartsAtUtc,
                    offering.EndsAtUtc,
                    offering.IsActive))
            .SingleOrDefaultAsync(cancellationToken);

        return data;
    }

    public async Task<IReadOnlyCollection<InstructorLookupDto>> SearchInstructorsAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query =
            from instructor in _dbContext.InstructorProfiles.AsNoTracking()
            join profile in _dbIUserAccountService.AsNoTracking()
                on instructor.UserId equals profile.Id
            select new
            {
                Instructor = instructor,
                Profile = profile
            };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var text = search.Trim();

            query = query.Where(x =>
                x.Instructor.PersonnelCode.Contains(text) ||
                (x.Profile.FirstName + " " + x.Profile.LastName)
                    .Contains(text));
        }

        return await query
            .OrderBy(x => x.Profile.FirstName)
            .ThenBy(x => x.Profile.LastName)
            .Take(50)
            .Select(x => new InstructorLookupDto(
                x.Instructor.Id,
                x.Profile.Id,
                x.Profile.FirstName + " " + x.Profile.LastName,
                x.Instructor.PersonnelCode))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<StudentLookupDto>> SearchStudentsAsync(
        Guid? majorId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query =
            from student in _dbContext.StudentProfiles.AsNoTracking()
            join profile in _dbIUserAccountService.AsNoTracking()
                on student.UserId equals profile.Id
            select new
            {
                Student = student,
                Profile = profile
            };

        if (majorId.HasValue)
        {
            query = query.Where(x =>
                x.Student.MajorId == majorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var text = search.Trim();

            query = query.Where(x =>
                x.Student.StudentNumber.Contains(text) ||
                (x.Profile.FirstName + " " + x.Profile.LastName)
                    .Contains(text));
        }

        return await query
            .OrderBy(x => x.Profile.FirstName)
            .ThenBy(x => x.Profile.LastName)
            .Take(100)
            .Select(x => new StudentLookupDto(
                x.Student.Id,
                x.Profile.Id,
                x.Profile.FirstName + " " + x.Profile.LastName,
                x.Student.StudentNumber,
                x.Student.MajorId))
            .ToListAsync(cancellationToken);
    }
}