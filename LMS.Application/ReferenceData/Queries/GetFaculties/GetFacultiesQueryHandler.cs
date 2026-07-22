using Application.Abstractions.Persistence;
using Application.Common.Models;
using Application.Common.Results;
using Common.Contracts.ReferenceData;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ReferenceData.Queries.GetFaculties;

public sealed class GetFacultiesQueryHandler
    : IRequestHandler<
        GetFacultiesQuery,
        Result<PagedResponse<FacultyListItemDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetFacultiesQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PagedResponse<FacultyListItemDto>>> Handle(
        GetFacultiesQuery request,
        CancellationToken cancellationToken)
    {
        var query =
            from faculty in _dbContext.Faculties.AsNoTracking()
            join university in _dbContext.Universities.AsNoTracking()
                on faculty.UniversityId equals university.Id
            select new
            {
                Faculty = faculty,
                UniversityTitle = university.Title
            };

        if (request.UniversityId.HasValue)
        {
            query = query.Where(
                x => x.Faculty.UniversityId == request.UniversityId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(x =>
                x.Faculty.Title.Contains(search) ||
                x.Faculty.Code.Contains(search) ||
                x.UniversityTitle.Contains(search));
        }

        query = request.SortBy?.Trim().ToLowerInvariant() switch
        {
            "university" when request.SortDescending =>
                query.OrderByDescending(x => x.UniversityTitle),

            "university" =>
                query.OrderBy(x => x.UniversityTitle),

            "code" when request.SortDescending =>
                query.OrderByDescending(x => x.Faculty.Code),

            "code" =>
                query.OrderBy(x => x.Faculty.Code),

            "status" when request.SortDescending =>
                query.OrderByDescending(x => x.Faculty.IsActive),

            "status" =>
                query.OrderBy(x => x.Faculty.IsActive),

            "title" when request.SortDescending =>
                query.OrderByDescending(x => x.Faculty.Title),

            _ when request.SortDescending =>
                query.OrderByDescending(x => x.Faculty.Title),

            _ =>
                query.OrderBy(x => x.Faculty.Title)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var pageNumber = Math.Max(request.PageNumber, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new FacultyListItemDto(
                x.Faculty.Id,
                x.Faculty.UniversityId,
                x.UniversityTitle,
                x.Faculty.Title,
                x.Faculty.Code,
                x.Faculty.IsActive))
            .ToListAsync(cancellationToken);

        return Result<PagedResponse<FacultyListItemDto>>.Success(
            new PagedResponse<FacultyListItemDto>(
                items,
                totalCount));
    }
}