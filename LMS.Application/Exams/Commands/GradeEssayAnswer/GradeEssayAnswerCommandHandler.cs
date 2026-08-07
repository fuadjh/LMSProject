using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Common.Results;
using Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Exams.Commands.GradeEssayAnswer;

public sealed class GradeEssayAnswerCommandHandler : IRequestHandler<GradeEssayAnswerCommand, Result>
{
    private readonly ICurrentUser _currentUser;
    private readonly IApplicationDbContext _dbContext;

    public GradeEssayAnswerCommandHandler(ICurrentUser currentUser, IApplicationDbContext dbContext)
    {
        _currentUser = currentUser;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(GradeEssayAnswerCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            return Result.Unauthorized("auth.required", "کاربر احراز هویت نشده است.");

        var instructorProfileId = await _dbContext.InstructorProfiles
            .Where(x => x.UserId == _currentUser.UserId.Value)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (!instructorProfileId.HasValue)
            return Result.Forbidden("instructor.required", "فقط استاد مجاز است.");

        var data = await (
            from a in _dbContext.ExamAnswers
            join s in _dbContext.ExamSubmissions on a.ExamSubmissionId equals s.Id
            join e in _dbContext.Exams on s.ExamId equals e.Id
            join q in _dbContext.Questions on a.QuestionId equals q.Id
            where a.Id == request.AnswerId
            select new
            {
                Answer = a,
                Submission = s,
                Exam = e,
                QuestionType = q.Type
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (data is null)
            return Result.NotFound("answer.not_found", "پاسخ یافت نشد.");

        if (data.Exam.CreatedByInstructorProfileId != instructorProfileId.Value)
            return Result.Forbidden("exam.access_denied", "این آزمون متعلق به استاد فعلی نیست.");

        if (data.QuestionType != QuestionType.Essay)
            return Result.Invalid(Error.Validation("answer", "فقط پاسخ تشریحی قابل تصحیح دستی است."));

        var maxScore = await _dbContext.ExamQuestions
            .Where(x => x.ExamId == data.Exam.Id && x.QuestionId == data.Answer.QuestionId)
            .Select(x => (decimal?)x.Score)
            .SingleOrDefaultAsync(cancellationToken);

        if (!maxScore.HasValue)
            return Result.NotFound("exam_question.not_found", "سوال آزمون یافت نشد.");

        if (request.AwardedScore > maxScore.Value)
            return Result.Invalid(Error.Validation("awardedScore", "نمره واردشده از نمره سوال بیشتر است."));

        data.Answer.Grade(request.AwardedScore);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var total = await _dbContext.ExamAnswers
            .Where(x => x.ExamSubmissionId == data.Submission.Id)
            .SumAsync(x => x.AwardedScore ?? 0m, cancellationToken);

        data.Submission.MarkGraded(total);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}