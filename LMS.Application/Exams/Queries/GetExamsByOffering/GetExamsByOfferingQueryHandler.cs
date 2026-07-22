using Application.Abstractions.Persistence;
using Application.Common.Results;
using LMS.Application.Exams;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Exams.Queries.GetExamsByOffering;

public sealed class GetExamsByOfferingQueryHandler : IRequestHandler<GetExamsByOfferingQuery, Result<IReadOnlyCollection<ExamLookupDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetExamsByOfferingQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyCollection<ExamLookupDto>>> Handle(GetExamsByOfferingQuery request, CancellationToken cancellationToken)
    {
        var items = await _dbContext.Exams
            .Where(x => x.CourseOfferingId == request.CourseOfferingId && x.IsActive)
            .OrderByDescending(x => x.StartsAtUtc)
            .Select(x => new ExamLookupDto(
                x.Id,
                x.Title,
                x.Status,
                x.DeliveryMode,
                x.StartsAtUtc,
                x.EndsAtUtc,
                x.DurationMinutes,
                x.MaxAttemptsPerStudent))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyCollection<ExamLookupDto>>.Success(items);
    }
}