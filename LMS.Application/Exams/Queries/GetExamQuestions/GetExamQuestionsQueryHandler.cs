using Application.Abstractions.Persistence;
using Application.Common.Results;
using LMS.Application.Exams;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Exams.Queries.GetExamQuestions;

public sealed class GetExamQuestionsQueryHandler : IRequestHandler<GetExamQuestionsQuery, Result<IReadOnlyCollection<ExamQuestionManageDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetExamQuestionsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyCollection<ExamQuestionManageDto>>> Handle(GetExamQuestionsQuery request, CancellationToken cancellationToken)
    {
        var items = await (
            from eq in _dbContext.ExamQuestions
            join q in _dbContext.Questions on eq.QuestionId equals q.Id
            where eq.ExamId == request.ExamId
            orderby eq.Order
            select new ExamQuestionManageDto(
                q.Id,
                q.Title,
                q.Type,
                eq.Score,
                eq.Order))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyCollection<ExamQuestionManageDto>>.Success(items);
    }
}