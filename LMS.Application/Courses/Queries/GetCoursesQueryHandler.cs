using Application.Abstractions.Persistence;
using Application.Common.Models;
using Application.Common.Results;
using Common.Contracts.Academic;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Courses.Queries.GetCourses;

public sealed class GetCoursesQueryHandler
    : IRequestHandler<
        GetCoursesQuery,
        Result<PagedResponse<CourseListItemDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCoursesQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PagedResponse<CourseListItemDto>>> Handle(
        GetCoursesQuery request,
        CancellationToken cancellationToken)
    {
        var query =
            from course in _dbContext.Courses.AsNoTracking()
            join major in _dbContext.Majors.AsNoTracking()
                on course.MajorId equals major.Id
            select new
            {
                Course = course,
                MajorTitle = major.Title
            };

        if (request.MajorId.HasValue)
        {
            query = query.Where(x =>
                x.Course.MajorId == request.MajorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(x =>
                x.Course.Title.Contains(search) ||
                x.Course.Code.Contains(search) ||
                x.MajorTitle.Contains(search));
        }

        query = request.SortBy?
            .Trim()
            .ToLowerInvariant() switch
        {
            "code" when request.SortDescending =>
                query.OrderByDescending(x => x.Course.Code),

            "code" =>
                query.OrderBy(x => x.Course.Code),

            "major" when request.SortDescending =>
                query.OrderByDescending(x => x.MajorTitle),

            "major" =>
                query.OrderBy(x => x.MajorTitle),

            "units" when request.SortDescending =>
                query.OrderByDescending(x => x.Course.Units),

            "units" =>
                query.OrderBy(x => x.Course.Units),

            "status" when request.SortDescending =>
                query.OrderByDescending(x => x.Course.IsActive),

            "status" =>
                query.OrderBy(x => x.Course.IsActive),

            "title" when request.SortDescending =>
                query.OrderByDescending(x => x.Course.Title),

            _ =>
                query.OrderBy(x => x.Course.Title)
        };

        var totalCount =
            await query.CountAsync(cancellationToken);

        var pageNumber = Math.Max(request.PageNumber, 1);

        var pageSize = Math.Clamp(
            request.PageSize,
            1,
            100);

        var items =
            await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CourseListItemDto(
                    x.Course.Id,
                    x.Course.MajorId,
                    x.MajorTitle,
                    x.Course.Title,
                    x.Course.Code,
                    x.Course.Units,
                    x.Course.EnrollmentScope,
                    x.Course.IsActive))
                .ToListAsync(cancellationToken);

        return Result<PagedResponse<CourseListItemDto>>.Success(
            new PagedResponse<CourseListItemDto>(
                items,
                totalCount));
    }
}