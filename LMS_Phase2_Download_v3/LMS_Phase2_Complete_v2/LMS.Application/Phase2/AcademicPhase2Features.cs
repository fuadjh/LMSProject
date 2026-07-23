using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Abstractions.Security;
using Application.Common.Models;
using Application.Common.Results;
using Common.Contracts.Academic;
using Common.Enums;
using Common.Security;
using Domain.Entities.Academics;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Courses.Commands.CreateCourse
{
    public sealed record CreateCourseCommand(
        Guid MajorId, string Title, string Code, int Units,
        CourseEnrollmentScope EnrollmentScope) : IRequest<Result<Guid>>;

    public sealed class CreateCourseCommandValidator
        : AbstractValidator<CreateCourseCommand>
    {
        public CreateCourseCommandValidator()
        {
            RuleFor(x => x.MajorId).NotEmpty();
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Units).InclusiveBetween(1, 30);
            RuleFor(x => x.EnrollmentScope).IsInEnum();
        }
    }

    public sealed class CreateCourseCommandHandler
        : IRequestHandler<CreateCourseCommand, Result<Guid>>
    {
        private readonly ICurrentUser _user;
        private readonly IAccessScopeService _scope;
        private readonly IApplicationDbContext _db;

        public CreateCourseCommandHandler(
            ICurrentUser user, IAccessScopeService scope,
            IApplicationDbContext db)
        {
            _user = user; _scope = scope; _db = db;
        }

        public async Task<Result<Guid>> Handle(
            CreateCourseCommand request, CancellationToken ct)
        {
            if (!_user.IsAuthenticated)
                return Result<Guid>.Unauthorized("auth.required", "کاربر احراز هویت نشده است.");

            if (!await _db.Majors.AnyAsync(
                    x => x.Id == request.MajorId && x.IsActive, ct))
                return Result<Guid>.NotFound("major.not_found", "رشته یافت نشد.");

            var admin = _user.Roles.Contains(
                RoleNames.Admin, StringComparer.OrdinalIgnoreCase);

            if (!admin && (!_user.UserProfileId.HasValue ||
                !await _scope.HasMajorAccessAsync(
                    _user.UserProfileId.Value, request.MajorId, ct)))
                return Result<Guid>.Forbidden("scope.denied", "دسترسی به رشته را ندارید.");

            var code = request.Code.Trim().ToUpperInvariant();

            if (await _db.Courses.AnyAsync(
                    x => x.MajorId == request.MajorId && x.Code == code, ct))
                return Result<Guid>.Conflict("course.code_exists", "کد درس تکراری است.");

            var entity = Course.Create(
                request.MajorId, request.Title, code,
                request.Units, request.EnrollmentScope);

            await _db.AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);
            return Result<Guid>.Success(entity.Id);
        }
    }
}

namespace Application.Courses.Commands.UpdateCourse
{
    public sealed record UpdateCourseCommand(
        Guid Id, Guid MajorId, string Title, string Code, int Units,
        CourseEnrollmentScope EnrollmentScope, bool IsActive)
        : IRequest<Result>;

    public sealed class UpdateCourseCommandValidator
        : AbstractValidator<UpdateCourseCommand>
    {
        public UpdateCourseCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.MajorId).NotEmpty();
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Units).InclusiveBetween(1, 30);
            RuleFor(x => x.EnrollmentScope).IsInEnum();
        }
    }

    public sealed class UpdateCourseCommandHandler
        : IRequestHandler<UpdateCourseCommand, Result>
    {
        private readonly ICurrentUser _user;
        private readonly IAccessScopeService _scope;
        private readonly IApplicationDbContext _db;

        public UpdateCourseCommandHandler(
            ICurrentUser user, IAccessScopeService scope,
            IApplicationDbContext db)
        {
            _user = user; _scope = scope; _db = db;
        }

        public async Task<Result> Handle(
            UpdateCourseCommand request, CancellationToken ct)
        {
            var course = await _db.Courses.SingleOrDefaultAsync(
                x => x.Id == request.Id, ct);

            if (course is null)
                return Result.NotFound("course.not_found", "درس یافت نشد.");

            if (!await _db.Majors.AnyAsync(
                    x => x.Id == request.MajorId && x.IsActive, ct))
                return Result.NotFound("major.not_found", "رشته یافت نشد.");

            var admin = _user.Roles.Contains(
                RoleNames.Admin, StringComparer.OrdinalIgnoreCase);

            if (!admin)
            {
                if (!_user.UserProfileId.HasValue)
                    return Result.Forbidden("profile.required", "پروفایل کاربر یافت نشد.");

                var currentAccess = await _scope.HasMajorAccessAsync(
                    _user.UserProfileId.Value, course.MajorId, ct);
                var targetAccess = course.MajorId == request.MajorId ||
                    await _scope.HasMajorAccessAsync(
                        _user.UserProfileId.Value, request.MajorId, ct);

                if (!currentAccess || !targetAccess)
                    return Result.Forbidden("scope.denied", "دسترسی لازم وجود ندارد.");
            }

            if (course.MajorId != request.MajorId &&
                await _db.CourseOfferings.AnyAsync(
                    x => x.CourseId == course.Id, ct))
                return Result.Conflict(
                    "course.has_offerings",
                    "درس دارای ارائه قابل انتقال نیست.");

            var code = request.Code.Trim().ToUpperInvariant();

            if (await _db.Courses.AnyAsync(
                    x => x.Id != request.Id &&
                         x.MajorId == request.MajorId &&
                         x.Code == code, ct))
                return Result.Conflict("course.code_exists", "کد درس تکراری است.");

            course.Update(
                request.MajorId, request.Title, code,
                request.Units, request.EnrollmentScope);

            if (request.IsActive) course.Activate(); else course.Deactivate();

            await _db.SaveChangesAsync(ct);
            return Result.Success();
        }
    }
}

namespace Application.Courses.Commands.DeleteCourse
{
    public sealed record DeleteCourseCommand(Guid Id) : IRequest<Result>;

    public sealed class DeleteCourseCommandValidator
        : AbstractValidator<DeleteCourseCommand>
    {
        public DeleteCourseCommandValidator() =>
            RuleFor(x => x.Id).NotEmpty();
    }

    public sealed class DeleteCourseCommandHandler
        : IRequestHandler<DeleteCourseCommand, Result>
    {
        private readonly ICurrentUser _user;
        private readonly IAccessScopeService _scope;
        private readonly IApplicationDbContext _db;

        public DeleteCourseCommandHandler(
            ICurrentUser user, IAccessScopeService scope,
            IApplicationDbContext db)
        {
            _user = user; _scope = scope; _db = db;
        }

        public async Task<Result> Handle(DeleteCourseCommand request, CancellationToken ct)
        {
            var course = await _db.Courses
                .Where(x => x.Id == request.Id)
                .Select(x => new { x.Id, x.MajorId })
                .SingleOrDefaultAsync(ct);

            if (course is null)
                return Result.NotFound("course.not_found", "درس یافت نشد.");

            var admin = _user.Roles.Contains(
                RoleNames.Admin, StringComparer.OrdinalIgnoreCase);

            if (!admin && (!_user.UserProfileId.HasValue ||
                !await _scope.HasMajorAccessAsync(
                    _user.UserProfileId.Value, course.MajorId, ct)))
                return Result.Forbidden("scope.denied", "دسترسی به رشته را ندارید.");

            if (await _db.CourseOfferings.AnyAsync(x => x.CourseId == request.Id, ct) ||
                await _db.QuestionBanks.AnyAsync(x => x.CourseId == request.Id, ct))
                return Result.Conflict("course.has_dependencies", "درس دارای اطلاعات وابسته است.");

            await _db.Courses.Where(x => x.Id == request.Id).ExecuteDeleteAsync(ct);
            return Result.Success();
        }
    }
}

namespace Application.Courses.Queries.GetCoursesPage
{
    public sealed record GetCoursesPageQuery
        : PagedQuery, IRequest<Result<PagedResponse<CourseListItemDto>>>
    {
        public Guid? MajorId { get; init; }
    }

    public sealed class GetCoursesPageQueryHandler
        : IRequestHandler<GetCoursesPageQuery,
            Result<PagedResponse<CourseListItemDto>>>
    {
        private readonly ICurrentUser _user;
        private readonly IApplicationDbContext _db;

        public GetCoursesPageQueryHandler(
            ICurrentUser user, IApplicationDbContext db)
        {
            _user = user; _db = db;
        }

        public async Task<Result<PagedResponse<CourseListItemDto>>> Handle(
            GetCoursesPageQuery request, CancellationToken ct)
        {
            var query =
                from c in _db.Courses.AsNoTracking()
                join m in _db.Majors.AsNoTracking() on c.MajorId equals m.Id
                select new { Course = c, Major = m };

            var admin = _user.Roles.Contains(
                RoleNames.Admin, StringComparer.OrdinalIgnoreCase);

            if (!admin)
            {
                if (!_user.UserProfileId.HasValue)
                    return Result<PagedResponse<CourseListItemDto>>.Forbidden(
                        "profile.required", "پروفایل کاربر یافت نشد.");

                var profileId = _user.UserProfileId.Value;
                query = query.Where(x =>
                    _db.UserMajorScopes.Any(s =>
                        s.UserProfileId == profileId &&
                        s.MajorId == x.Course.MajorId) ||
                    _db.UserFacultyScopes.Any(s =>
                        s.UserProfileId == profileId &&
                        s.FacultyId == x.Major.FacultyId));
            }

            if (request.MajorId.HasValue)
                query = query.Where(x => x.Course.MajorId == request.MajorId.Value);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(x =>
                    x.Course.Title.Contains(search) ||
                    x.Course.Code.Contains(search) ||
                    x.Major.Title.Contains(search));
            }

            query = request.SortBy?.ToLowerInvariant() switch
            {
                "major" when request.SortDescending => query.OrderByDescending(x => x.Major.Title),
                "major" => query.OrderBy(x => x.Major.Title),
                "code" when request.SortDescending => query.OrderByDescending(x => x.Course.Code),
                "code" => query.OrderBy(x => x.Course.Code),
                "units" when request.SortDescending => query.OrderByDescending(x => x.Course.Units),
                "units" => query.OrderBy(x => x.Course.Units),
                _ when request.SortDescending => query.OrderByDescending(x => x.Course.Title),
                _ => query.OrderBy(x => x.Course.Title)
            };

            var total = await query.CountAsync(ct);
            var page = Math.Max(request.PageNumber, 1);
            var size = Math.Clamp(request.PageSize, 1, 100);

            var items = await query.Skip((page - 1) * size).Take(size)
                .Select(x => new CourseListItemDto(
                    x.Course.Id, x.Course.MajorId, x.Major.Title,
                    x.Course.Title, x.Course.Code, x.Course.Units,
                    x.Course.EnrollmentScope, x.Course.IsActive))
                .ToListAsync(ct);

            return Result<PagedResponse<CourseListItemDto>>.Success(
                new PagedResponse<CourseListItemDto>(items, total));
        }
    }
}

namespace Application.Semesters.Commands.UpdateSemester
{
    public sealed record UpdateSemesterCommand(
        Guid Id, string Title, DateTime StartsAtUtc,
        DateTime EndsAtUtc, bool IsActive) : IRequest<Result>;

    public sealed class UpdateSemesterCommandValidator
        : AbstractValidator<UpdateSemesterCommand>
    {
        public UpdateSemesterCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
            RuleFor(x => x.EndsAtUtc).GreaterThan(x => x.StartsAtUtc);
        }
    }

    public sealed class UpdateSemesterCommandHandler
        : IRequestHandler<UpdateSemesterCommand, Result>
    {
        private readonly IApplicationDbContext _db;
        public UpdateSemesterCommandHandler(IApplicationDbContext db) => _db = db;

        public async Task<Result> Handle(UpdateSemesterCommand request, CancellationToken ct)
        {
            var entity = await _db.Semesters.SingleOrDefaultAsync(
                x => x.Id == request.Id, ct);

            if (entity is null)
                return Result.NotFound("semester.not_found", "نیم‌سال یافت نشد.");

            if (await _db.Semesters.AnyAsync(
                    x => x.Id != request.Id &&
                         x.Title == request.Title.Trim(), ct))
                return Result.Conflict("semester.title_exists", "عنوان نیم‌سال تکراری است.");

            entity.Update(request.Title, request.StartsAtUtc, request.EndsAtUtc);
            if (request.IsActive) entity.Activate(); else entity.Deactivate();

            await _db.SaveChangesAsync(ct);
            return Result.Success();
        }
    }
}

namespace Application.Semesters.Commands.DeleteSemester
{
    public sealed record DeleteSemesterCommand(Guid Id) : IRequest<Result>;

    public sealed class DeleteSemesterCommandHandler
        : IRequestHandler<DeleteSemesterCommand, Result>
    {
        private readonly IApplicationDbContext _db;
        public DeleteSemesterCommandHandler(IApplicationDbContext db) => _db = db;

        public async Task<Result> Handle(DeleteSemesterCommand request, CancellationToken ct)
        {
            if (!await _db.Semesters.AnyAsync(x => x.Id == request.Id, ct))
                return Result.NotFound("semester.not_found", "نیم‌سال یافت نشد.");

            if (await _db.CourseOfferings.AnyAsync(x => x.SemesterId == request.Id, ct))
                return Result.Conflict("semester.has_offerings", "نیم‌سال دارای گروه درسی است.");

            await _db.Semesters.Where(x => x.Id == request.Id).ExecuteDeleteAsync(ct);
            return Result.Success();
        }
    }
}

namespace Application.Semesters.Queries.GetSemestersPage
{
    public sealed record GetSemestersPageQuery
        : PagedQuery, IRequest<Result<PagedResponse<SemesterListItemDto>>>;

    public sealed class GetSemestersPageQueryHandler
        : IRequestHandler<GetSemestersPageQuery,
            Result<PagedResponse<SemesterListItemDto>>>
    {
        private readonly IApplicationDbContext _db;
        public GetSemestersPageQueryHandler(IApplicationDbContext db) => _db = db;

        public async Task<Result<PagedResponse<SemesterListItemDto>>> Handle(
            GetSemestersPageQuery request, CancellationToken ct)
        {
            var query = _db.Semesters.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Search))
                query = query.Where(x => x.Title.Contains(request.Search.Trim()));

            query = request.SortBy?.ToLowerInvariant() switch
            {
                "title" when request.SortDescending => query.OrderByDescending(x => x.Title),
                "title" => query.OrderBy(x => x.Title),
                "end" when request.SortDescending => query.OrderByDescending(x => x.EndsAtUtc),
                "end" => query.OrderBy(x => x.EndsAtUtc),
                _ when request.SortDescending => query.OrderByDescending(x => x.StartsAtUtc),
                _ => query.OrderBy(x => x.StartsAtUtc)
            };

            var total = await query.CountAsync(ct);
            var page = Math.Max(request.PageNumber, 1);
            var size = Math.Clamp(request.PageSize, 1, 100);

            var items = await query.Skip((page - 1) * size).Take(size)
                .Select(x => new SemesterListItemDto(
                    x.Id, x.Title, x.StartsAtUtc, x.EndsAtUtc, x.IsActive))
                .ToListAsync(ct);

            return Result<PagedResponse<SemesterListItemDto>>.Success(
                new PagedResponse<SemesterListItemDto>(items, total));
        }
    }
}

namespace Application.CourseOfferings.Commands.UpdateCourseOffering
{
    public sealed record UpdateCourseOfferingCommand(
        Guid Id, string SectionCode, int Capacity, bool IsActive)
        : IRequest<Result>;

    public sealed class UpdateCourseOfferingCommandValidator
        : AbstractValidator<UpdateCourseOfferingCommand>
    {
        public UpdateCourseOfferingCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.SectionCode).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Capacity).InclusiveBetween(1, 500);
        }
    }

    public sealed class UpdateCourseOfferingCommandHandler
        : IRequestHandler<UpdateCourseOfferingCommand, Result>
    {
        private readonly ICurrentUser _user;
        private readonly IAccessScopeService _scope;
        private readonly IApplicationDbContext _db;

        public UpdateCourseOfferingCommandHandler(
            ICurrentUser user, IAccessScopeService scope,
            IApplicationDbContext db)
        {
            _user = user; _scope = scope; _db = db;
        }

        public async Task<Result> Handle(
            UpdateCourseOfferingCommand request, CancellationToken ct)
        {
            var data = await (
                from o in _db.CourseOfferings
                join c in _db.Courses on o.CourseId equals c.Id
                where o.Id == request.Id
                select new { Offering = o, c.MajorId })
                .SingleOrDefaultAsync(ct);

            if (data is null)
                return Result.NotFound("offering.not_found", "گروه یافت نشد.");

            var admin = _user.Roles.Contains(
                RoleNames.Admin, StringComparer.OrdinalIgnoreCase);

            if (!admin && (!_user.UserProfileId.HasValue ||
                !await _scope.HasMajorAccessAsync(
                    _user.UserProfileId.Value, data.MajorId, ct)))
                return Result.Forbidden("scope.denied", "دسترسی به رشته را ندارید.");

            var section = request.SectionCode.Trim().ToUpperInvariant();

            if (await _db.CourseOfferings.AnyAsync(
                    x => x.Id != request.Id &&
                         x.CourseId == data.Offering.CourseId &&
                         x.SemesterId == data.Offering.SemesterId &&
                         x.SectionCode == section, ct))
                return Result.Conflict("offering.section_exists", "کد گروه تکراری است.");

            var count = await _db.Enrollments.CountAsync(
                x => x.CourseOfferingId == request.Id && x.IsActive, ct);

            data.Offering.Update(section, request.Capacity, count);
            if (request.IsActive) data.Offering.Activate(); else data.Offering.Deactivate();

            await _db.SaveChangesAsync(ct);
            return Result.Success();
        }
    }
}

namespace Application.CourseOfferings.Commands.DeleteCourseOffering
{
    public sealed record DeleteCourseOfferingCommand(Guid Id) : IRequest<Result>;

    public sealed class DeleteCourseOfferingCommandHandler
        : IRequestHandler<DeleteCourseOfferingCommand, Result>
    {
        private readonly IApplicationDbContext _db;
        public DeleteCourseOfferingCommandHandler(IApplicationDbContext db) => _db = db;

        public async Task<Result> Handle(
            DeleteCourseOfferingCommand request, CancellationToken ct)
        {
            if (!await _db.CourseOfferings.AnyAsync(x => x.Id == request.Id, ct))
                return Result.NotFound("offering.not_found", "گروه یافت نشد.");

            if (await _db.Enrollments.AnyAsync(x => x.CourseOfferingId == request.Id, ct) ||
                await _db.Exams.AnyAsync(x => x.CourseOfferingId == request.Id, ct))
                return Result.Conflict("offering.has_dependencies", "گروه دارای اطلاعات وابسته است.");

            await _db.CourseOfferings
                .Where(x => x.Id == request.Id)
                .ExecuteDeleteAsync(ct);

            return Result.Success();
        }
    }
}

namespace Application.CourseOfferings.Commands.EnrollStudent
{
    public sealed class EnrollStudentCommandHandler
        : IRequestHandler<EnrollStudentCommand, Result>
    {
        private readonly ICurrentUser _user;
        private readonly IAccessScopeService _scope;
        private readonly IApplicationDbContext _db;

        public EnrollStudentCommandHandler(
            ICurrentUser user, IAccessScopeService scope,
            IApplicationDbContext db)
        {
            _user = user; _scope = scope; _db = db;
        }

        public async Task<Result> Handle(EnrollStudentCommand request, CancellationToken ct)
        {
            var data = await (
                from o in _db.CourseOfferings
                join c in _db.Courses on o.CourseId equals c.Id
                where o.Id == request.CourseOfferingId &&
                      o.IsActive && c.IsActive
                select new { Offering = o, Course = c })
                .SingleOrDefaultAsync(ct);

            if (data is null)
                return Result.NotFound("offering.not_found", "گروه یافت نشد.");

            var admin = _user.Roles.Contains(
                RoleNames.Admin, StringComparer.OrdinalIgnoreCase);

            if (!admin && (!_user.UserProfileId.HasValue ||
                !await _scope.HasMajorAccessAsync(
                    _user.UserProfileId.Value, data.Course.MajorId, ct)))
                return Result.Forbidden("scope.denied", "دسترسی به رشته را ندارید.");

            var student = await (
                from s in _db.StudentProfiles
                join p in _db.UserProfiles on s.UserProfileId equals p.Id
                where s.Id == request.StudentProfileId && p.IsActive
                select s).SingleOrDefaultAsync(ct);

            if (student is null)
                return Result.NotFound("student.not_found", "دانشجو یافت نشد.");

            if (!data.Course.AllowsEnrollmentFor(student.MajorId))
                return Result.Forbidden(
                    "student.major_not_allowed",
                    "رشته دانشجو مجاز نیست.");

            var existing = await _db.Enrollments.SingleOrDefaultAsync(
                x => x.CourseOfferingId == request.CourseOfferingId &&
                     x.StudentProfileId == request.StudentProfileId, ct);

            if (existing?.IsActive == true)
                return Result.Conflict("enrollment.exists", "ثبت‌نام قبلاً انجام شده است.");

            var activeCount = await _db.Enrollments.CountAsync(
                x => x.CourseOfferingId == request.CourseOfferingId &&
                     x.IsActive, ct);

            if (!data.Offering.HasAvailableCapacity(activeCount))
                return Result.Conflict("offering.capacity_full", "ظرفیت تکمیل است.");

            if (existing is null)
                await _db.AddAsync(
                    Enrollment.Create(
                        request.CourseOfferingId, request.StudentProfileId), ct);
            else
                existing.Activate();

            await _db.SaveChangesAsync(ct);
            return Result.Success();
        }
    }
}
