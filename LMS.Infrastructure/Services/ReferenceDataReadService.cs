using Application.Abstractions.Read;
using Application.Common.Models;

using Common.Contracts.ReferenceData;
using Domain.Entities.Academics;
using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class ReferenceDataReadService : IReferenceDataReadService
{
    private readonly LmsDbContext _dbContext;

    public ReferenceDataReadService(LmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<LookupItemDto>> GetFacultiesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Faculties
            .Where(x => x.IsActive)
            .OrderBy(x => x.Title)
            .Select(x => new LookupItemDto(x.Id, x.Title))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<LookupItemDto>> GetMajorsAsync(
        Guid? facultyId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Majors.Where(x => x.IsActive);

        if (facultyId.HasValue)
            query = query.Where(x => x.FacultyId == facultyId.Value);

        return await query
            .OrderBy(x => x.Title)
            .Select(x => new LookupItemDto(x.Id, x.Title))
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResponse<UniversityListItemDto>> GetUniversitiesAsync(
        PagedQuery request,
        CancellationToken cancellationToken = default)
    {
        IQueryable<University> query = _dbContext.Universities
            .AsNoTracking();

        var search = request.Search?.Trim();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Title.Contains(search) ||
                x.Code.Contains(search));
        }

        query = ApplyUniversitySorting(
            query,
            request.SortBy,
            request.SortDescending);

        var totalCount = await query.CountAsync(cancellationToken);

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UniversityListItemDto(
                x.Id,
                x.Title,
                x.Code,
                x.IsActive))
            .ToListAsync(cancellationToken);

        return new PagedResponse<UniversityListItemDto>(
            items,
            totalCount, pageNumber, pageSize);
    }

    private static IQueryable<University> ApplyUniversitySorting(
        IQueryable<University> query,
        string? sortBy,
        bool descending)
    {
        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "code" when descending => query.OrderByDescending(x => x.Code),
            "code" => query.OrderBy(x => x.Code),

            "title" when descending => query.OrderByDescending(x => x.Title),
            _ when descending => query.OrderByDescending(x => x.Title),

            _ => query.OrderBy(x => x.Title)
        };
    }
}