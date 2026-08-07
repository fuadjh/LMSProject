using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Common.Results;
using Common.Enums;
using Domain.Entities.Exams;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Exams.Commands.SubmitExamAttempt;

public sealed class SubmitExamAttemptCommandHandler : IRequestHandler<SubmitExamAttemptCommand, Result>
{
    private readonly ICurrentUser _currentUser;
    private readonly IApplicationDbContext _dbContext;

    public SubmitExamAttemptCommandHandler(ICurrentUser currentUser, IApplicationDbContext dbContext)
    {
        _currentUser = currentUser;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(SubmitExamAttemptCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            return Result.Unauthorized("auth.required", "کاربر احراز هویت نشده است.");

        var studentProfileId = await _dbContext.StudentProfiles
            .Where(x => x.UserId == _currentUser.UserId.Value)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (!studentProfileId.HasValue)
            return Result.Forbidden("student.required", "فقط دانشجو مجاز است.");

        var submission = await _dbContext.ExamSubmissions
            .SingleOrDefaultAsync(x => x.Id == request.SubmissionId && x.StudentProfileId == studentProfileId.Value, cancellationToken);

        if (submission is null)
            return Result.NotFound("submission.not_found", "Attempt یافت نشد.");

        if (submission.Status != ExamSubmissionStatus.InProgress)
            return Result.Forbidden("submission.not_editable", "این attempt قابل ارسال نیست.");

        var exam = await _dbContext.Exams
            .SingleAsync(x => x.Id == submission.ExamId, cancellationToken);

        if (exam.DeliveryMode != ExamDeliveryMode.Online)
            return Result.Failure("exam.center_based_not_supported", "اجرای آزمون مرکز-محور در این Patch فعال نشده است.");

        if (DateTime.UtcNow > exam.EndsAtUtc)
            return Result.Forbidden("exam.time_over", "زمان آزمون به پایان رسیده است.");

        var startedPlusDuration = submission.StartedAtUtc.AddMinutes(exam.DurationMinutes);
        if (DateTime.UtcNow > startedPlusDuration)
            return Result.Forbidden("exam.duration_over", "زمان مجاز attempt به پایان رسیده است.");

        var examQuestions = await (
            from eq in _dbContext.ExamQuestions
            join q in _dbContext.Questions on eq.QuestionId equals q.Id
            where eq.ExamId == submission.ExamId
            select new
            {
                eq.QuestionId,
                eq.Score,
                q.Type
            })
            .ToListAsync(cancellationToken);

        await using var tx = await _dbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var item in request.Answers)
            {
                var examQuestion = examQuestions.SingleOrDefault(x => x.QuestionId == item.QuestionId);
                if (examQuestion is null)
                    continue;

                decimal? awardedScore = null;

                if (examQuestion.Type == QuestionType.MultipleChoice)
                {
                    if (item.SelectedOptionId.HasValue)
                    {
                        var isCorrect = await _dbContext.QuestionOptions
                            .AnyAsync(x => x.Id == item.SelectedOptionId.Value &&
                                           x.QuestionId == item.QuestionId &&
                                           x.IsCorrect,
                                cancellationToken);

                        awardedScore = isCorrect ? examQuestion.Score : 0m;
                    }
                    else
                    {
                        awardedScore = 0m;
                    }
                }
                else
                {
                    var hasEssayData = !string.IsNullOrWhiteSpace(item.EssayText) ||
                                       !string.IsNullOrWhiteSpace(item.EssayAttachmentPath);

                    if (!hasEssayData)
                    {
                        // خالی هم مجاز است، نمره فعلاً null می‌ماند
                    }
                }

                var answer = ExamAnswer.Create(
                    submission.Id,
                    item.QuestionId,
                    item.SelectedOptionId,
                    item.EssayText,
                    item.EssayAttachmentFileName,
                    item.EssayAttachmentPath,
                    awardedScore);

                await _dbContext.AddAsync(answer, cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            var totalAutoScore = await _dbContext.ExamAnswers
                .Where(x => x.ExamSubmissionId == submission.Id)
                .SumAsync(x => x.AwardedScore ?? 0m, cancellationToken);

            submission.Submit(totalAutoScore);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return Result.Failure("exam.submit_failed", ex.Message);
        }
    }
}