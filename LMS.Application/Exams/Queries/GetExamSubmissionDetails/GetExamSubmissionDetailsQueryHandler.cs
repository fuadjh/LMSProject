using Application.Abstractions.Identity;
using Application.Abstractions.Persistence;
using Application.Common.Results;
using LMS.Application.Exams;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Exams.Queries.GetExamSubmissionDetails;

public sealed class GetExamSubmissionDetailsQueryHandler
    : IRequestHandler<
        GetExamSubmissionDetailsQuery,
        Result<ExamSubmissionDetailsDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAccountService _userAccountService;

    public GetExamSubmissionDetailsQueryHandler(
        IApplicationDbContext dbContext,
        IUserAccountService userAccountService)
    {
        _dbContext = dbContext;
        _userAccountService = userAccountService;
    }

    public async Task<Result<ExamSubmissionDetailsDto>> Handle(
        GetExamSubmissionDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var header = await (
            from submission in _dbContext.ExamSubmissions
            join students in _dbContext.StudentProfiles
                on submission.StudentProfileId equals students.Id
            where submission.Id == request.SubmissionId
            select new
            {
                SubmissionId = submission.Id,
                StudentUserId = students.Id,
                students.StudentNumber,
                submission.AttemptNumber,
                submission.StartedAtUtc,
                submission.SubmittedAtUtc,
                submission.TotalScore
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (header is null)
        {
            return Result<ExamSubmissionDetailsDto>.NotFound(
                "submission.not_found",
                "ارسال آزمون یافت نشد.");
        }

        var student = await _userAccountService.FindByIdAsync(
            header.StudentUserId,
            cancellationToken);

        if (student is null)
        {
            return Result<ExamSubmissionDetailsDto>.NotFound(
                "submission.student_not_found",
                "کاربر دانشجوی این ارسال آزمون یافت نشد.");
        }

        var answers = await (
            from answer in _dbContext.ExamAnswers
            join question in _dbContext.Questions
                on answer.QuestionId equals question.Id
            join examQuestion in _dbContext.ExamQuestions
                on question.Id equals examQuestion.QuestionId
            where answer.ExamSubmissionId == request.SubmissionId
            select new
            {
                AnswerId = answer.Id,
                answer.QuestionId,
                question.Title,
                question.Type,
                answer.SelectedOptionId,
                answer.EssayText,
                answer.EssayAttachmentFileName,
                answer.EssayAttachmentPath,
                answer.AwardedScore,
                examQuestion.Score
            })
            .ToListAsync(cancellationToken);

        var optionIds = answers
            .Where(answer => answer.SelectedOptionId.HasValue)
            .Select(answer => answer.SelectedOptionId!.Value)
            .Distinct()
            .ToArray();

        var optionMap = optionIds.Length == 0
            ? new Dictionary<Guid, string>()
            : await _dbContext.QuestionOptions
                .Where(option => optionIds.Contains(option.Id))
                .ToDictionaryAsync(
                    option => option.Id,
                    option => option.Text,
                    cancellationToken);

        var answerDtos = answers
            .Select(answer =>
            {
                string? selectedOptionText = null;

                if (answer.SelectedOptionId.HasValue)
                {
                    optionMap.TryGetValue(
                        answer.SelectedOptionId.Value,
                        out selectedOptionText);
                }

                return new ExamSubmissionAnswerDto(
                    answer.AnswerId,
                    answer.QuestionId,
                    answer.Title,
                    answer.Type,
                    answer.Score,
                    selectedOptionText,
                    answer.EssayText,
                    answer.EssayAttachmentFileName,
                    answer.EssayAttachmentPath,
                    answer.AwardedScore);
            })
            .ToArray();

        var details = new ExamSubmissionDetailsDto(
            header.SubmissionId,
            $"{student.FirstName} {student.LastName}".Trim(),
            header.StudentNumber,
            header.AttemptNumber,
            header.StartedAtUtc,
            header.SubmittedAtUtc,
            header.TotalScore,
            answerDtos);

        return Result<ExamSubmissionDetailsDto>.Success(details);
    }
}