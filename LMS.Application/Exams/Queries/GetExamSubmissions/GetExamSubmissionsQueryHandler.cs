using Application.Abstractions.Persistence;
using Application.Common.Results;
using LMS.Application.Exams;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Exams.Queries.GetExamSubmissions;

public sealed class GetExamSubmissionsQueryHandler : IRequestHandler<GetExamSubmissionsQuery, Result<IReadOnlyCollection<ExamSubmissionSummaryDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetExamSubmissionsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyCollection<ExamSubmissionSummaryDto>>> Handle(GetExamSubmissionsQuery request, CancellationToken cancellationToken)
    {
        var items = await (
            from s in _dbContext.ExamSubmissions
            join st in _dbContext.StudentProfiles on s.StudentProfileId equals st.Id
            join p in _dbIUserAccountService on st.UserId equals p.Id
            where s.ExamId == request.ExamId
            orderby s.AttemptNumber descending
            select new ExamSubmissionSummaryDto(
                s.Id,
                p.FirstName + " " + p.LastName,
                st.StudentNumber,
                s.AttemptNumber,
                s.StartedAtUtc,
                s.SubmittedAtUtc,
                s.TotalScore))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyCollection<ExamSubmissionSummaryDto>>.Success(items);
    }
}