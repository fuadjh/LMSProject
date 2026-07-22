using Application.Abstractions.Persistence;
using Application.Common.Results;
using LMS.Application.Exams;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Exams.Queries.GetExamForStudent;

public sealed class GetExamForStudentQueryHandler : IRequestHandler<GetExamForStudentQuery, Result<ExamForStudentDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetExamForStudentQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ExamForStudentDto>> Handle(GetExamForStudentQuery request, CancellationToken cancellationToken)
    {
        var exam = await _dbContext.Exams
            .Where(x => x.Id == request.ExamId && x.IsActive)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.DeliveryMode,
                x.DurationMinutes,
                x.StartsAtUtc,
                x.EndsAtUtc
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (exam is null)
            return Result<ExamForStudentDto>.NotFound("exam.not_found", "آزمون یافت نشد.");

        var questions = await (
            from eq in _dbContext.ExamQuestions
            join q in _dbContext.Questions on eq.QuestionId equals q.Id
            where eq.ExamId == request.ExamId
            orderby eq.Order
            select new ExamForStudentQuestionDto(
                q.Id,
                q.Title,
                q.Body,
                q.Type,
                eq.Score,
                q.AttachmentFileName,
                q.AttachmentPath,
                Array.Empty<ExamForStudentOptionDto>()))
            .ToListAsync(cancellationToken);

        var questionIds = questions.Select(x => x.QuestionId).ToArray();

        var options = await _dbContext.QuestionOptions
            .Where(x => questionIds.Contains(x.QuestionId))
            .OrderBy(x => x.Order)
            .Select(x => new { x.QuestionId, Item = new ExamForStudentOptionDto(x.Id, x.Text, x.Order) })
            .ToListAsync(cancellationToken);

        var finalQuestions = questions
            .Select(q => q with
            {
                Options = options.Where(x => x.QuestionId == q.QuestionId).Select(x => x.Item).ToArray()
            })
            .ToArray();

        return Result<ExamForStudentDto>.Success(
            new ExamForStudentDto(
                exam.Id,
                exam.Title,
                exam.DeliveryMode,
                exam.DurationMinutes,
                exam.StartsAtUtc,
                exam.EndsAtUtc,
                finalQuestions));
    }
}