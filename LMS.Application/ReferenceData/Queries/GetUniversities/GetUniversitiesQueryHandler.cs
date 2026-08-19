using Application.Abstractions.Persistence;
using Application.Common.Models;
using Application.Common.Results;
using Common.Contracts.ReferenceData;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ReferenceData.Queries.GetUniversities;

public sealed class GetUniversitiesQueryHandler
    : IRequestHandler<
        GetUniversitiesQuery,
        Result<PagedResponse<UniversityLookupDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetUniversitiesQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PagedResponse<UniversityLookupDto>>> Handle(
        GetUniversitiesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Universities
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(x =>
                x.Title.Contains(search) ||
                x.Code.Contains(search));
        }

        query = request.SortBy?.Trim().ToLowerInvariant() switch
        {
            "code" when request.SortDescending =>
                query.OrderByDescending(x => x.Code),

            "code" =>
                query.OrderBy(x => x.Code),

            "title" when request.SortDescending =>
                query.OrderByDescending(x => x.Title),

            _ when request.SortDescending =>
                query.OrderByDescending(x => x.Title),

            _ =>
                query.OrderBy(x => x.Title)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var pageNumber = Math.Max(request.PageNumber, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UniversityLookupDto(
                x.Id,
                x.Title,
                x.Code,
                x.IsActive))
            .ToListAsync(cancellationToken);

        return Result<PagedResponse<UniversityLookupDto>>.Success(
            new PagedResponse<UniversityLookupDto>(
                items,
        totalCount,
        pageNumber,
        pageSize));
    }
}