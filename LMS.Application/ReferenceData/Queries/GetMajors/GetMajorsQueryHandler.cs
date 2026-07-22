using Application.Abstractions.Persistence;
using Application.Common.Models;
using Application.Common.Results;
using Common.Contracts.ReferenceData;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ReferenceData.Queries.GetMajors;

public sealed class GetMajorsQueryHandler
    : IRequestHandler<
        GetMajorsQuery,
        Result<PagedResponse<MajorListItemDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetMajorsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PagedResponse<MajorListItemDto>>> Handle(
        GetMajorsQuery request,
        CancellationToken cancellationToken)
    {
        var query =
            from major in _dbContext.Majors.AsNoTracking()
            join faculty in _dbContext.Faculties.AsNoTracking()
                on major.FacultyId equals faculty.Id
            join university in _dbContext.Universities.AsNoTracking()
                on faculty.UniversityId equals university.Id
            select new
            {
                Major = major,
                FacultyTitle = faculty.Title,
                UniversityId = faculty.UniversityId,
                UniversityTitle = university.Title
            };

        if (request.FacultyId.HasValue)
        {
            query = query.Where(
                x => x.Major.FacultyId == request.FacultyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(x =>
                x.Major.Title.Contains(search) ||
                x.Major.Code.Contains(search) ||
                x.FacultyTitle.Contains(search) ||
                x.UniversityTitle.Contains(search));
        }

        query = request.SortBy?.Trim().ToLowerInvariant() switch
        {
            "university" when request.SortDescending =>
                query.OrderByDescending(x => x.UniversityTitle),

            "university" =>
                query.OrderBy(x => x.UniversityTitle),

            "faculty" when request.SortDescending =>
                query.OrderByDescending(x => x.FacultyTitle),

            "faculty" =>
                query.OrderBy(x => x.FacultyTitle),

            "code" when request.SortDescending =>
                query.OrderByDescending(x => x.Major.Code),

            "code" =>
                query.OrderBy(x => x.Major.Code),

            "status" when request.SortDescending =>
                query.OrderByDescending(x => x.Major.IsActive),

            "status" =>
                query.OrderBy(x => x.Major.IsActive),

            "title" when request.SortDescending =>
                query.OrderByDescending(x => x.Major.Title),

            _ when request.SortDescending =>
                query.OrderByDescending(x => x.Major.Title),

            _ =>
                query.OrderBy(x => x.Major.Title)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var pageNumber = Math.Max(request.PageNumber, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new MajorListItemDto(
                x.Major.Id,
                x.Major.FacultyId,
                x.FacultyTitle,
                x.UniversityId,
                x.UniversityTitle,
                x.Major.Title,
                x.Major.Code,
                x.Major.IsActive))
            .ToListAsync(cancellationToken);

        return Result<PagedResponse<MajorListItemDto>>.Success(
            new PagedResponse<MajorListItemDto>(
                items,
                totalCount));
    }
}