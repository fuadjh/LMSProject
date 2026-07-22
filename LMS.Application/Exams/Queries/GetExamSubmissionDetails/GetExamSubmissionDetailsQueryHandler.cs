using Application.Abstractions.Persistence;
using Application.Common.Results;
using LMS.Application.Exams;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Exams.Queries.GetExamSubmissionDetails;

public sealed class GetExamSubmissionDetailsQueryHandler : IRequestHandler<GetExamSubmissionDetailsQuery, Result<ExamSubmissionDetailsDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetExamSubmissionDetailsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ExamSubmissionDetailsDto>> Handle(GetExamSubmissionDetailsQuery request, CancellationToken cancellationToken)
    {
        var header = await (
            from s in _dbContext.ExamSubmissions
            join st in _dbContext.StudentProfiles on s.StudentProfileId equals st.Id
            join p in _dbContext.UserProfiles on st.UserProfileId equals p.Id
            where s.Id == request.SubmissionId
            select new
            {
                s.Id,
                StudentFullName = p.FirstName + " " + p.LastName,
                st.StudentNumber,
                s.AttemptNumber,
                s.StartedAtUtc,
                s.SubmittedAtUtc,
                s.TotalScore
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (header is null)
            return Result<ExamSubmissionDetailsDto>.NotFound("submission.not_found", "Submission یافت نشد.");

        var answers = await (
            from a in _dbContext.ExamAnswers
            join q in _dbContext.Questions on a.QuestionId equals q.Id
            join eq in _dbContext.ExamQuestions on q.Id equals eq.QuestionId
            where a.ExamSubmissionId == request.SubmissionId
            select new
            {
                a.Id,
                a.QuestionId,
                q.Title,
                q.Type,
                a.SelectedOptionId,
                a.EssayText,
                a.EssayAttachmentFileName,
                a.EssayAttachmentPath,
                a.AwardedScore,
                eq.Score
            })
            .ToListAsync(cancellationToken);

        var optionIds = answers.Where(x => x.SelectedOptionId.HasValue).Select(x => x.SelectedOptionId!.Value).ToArray();

        var optionMap = await _dbContext.QuestionOptions
            .Where(x => optionIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Text, cancellationToken);

        var result = new ExamSubmissionDetailsDto(
            header.Id,
            header.StudentFullName,
            header.StudentNumber,
            header.AttemptNumber,
            header.StartedAtUtc,
            header.SubmittedAtUtc,
            header.TotalScore,
            answers.Select(x => new ExamSubmissionAnswerDto(
                x.Id,
                x.QuestionId,
                x.Title,
                x.Type,
                x.Score,
                x.SelectedOptionId.HasValue && optionMap.ContainsKey(x.SelectedOptionId.Value) ? optionMap[x.SelectedOptionId.Value] : null,
                x.EssayText,
                x.EssayAttachmentFileName,
                x.EssayAttachmentPath,
                x.AwardedScore)).ToArray());

        return Result<ExamSubmissionDetailsDto>.Success(result);
    }
}