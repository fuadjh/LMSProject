using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Common.Results;
using Common.Enums;
using Domain.Entities.Exams;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Exams.Commands.StartExamAttempt;

public sealed class StartExamAttemptCommandHandler : IRequestHandler<StartExamAttemptCommand, Result<Guid>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IApplicationDbContext _dbContext;

    public StartExamAttemptCommandHandler(ICurrentUser currentUser, IApplicationDbContext dbContext)
    {
        _currentUser = currentUser;
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(StartExamAttemptCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            return Result<Guid>.Unauthorized("auth.required", "کاربر احراز هویت نشده است.");

        var studentProfileId = await _dbContext.StudentProfiles
            .Where(x => x.Id == _currentUser.UserId.Value)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (!studentProfileId.HasValue)
            return Result<Guid>.Forbidden("student.required", "فقط دانشجو می‌تواند در آزمون شرکت کند.");

        var examData = await (
            from e in _dbContext.Exams
            join o in _dbContext.CourseOfferings on e.CourseOfferingId equals o.Id
            where e.Id == request.ExamId && e.IsActive
            select new
            {
                Exam = e,
                o.Id
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (examData is null)
            return Result<Guid>.NotFound("exam.not_found", "آزمون یافت نشد.");

        if (examData.Exam.DeliveryMode != ExamDeliveryMode.Online)
            return Result<Guid>.Failure("exam.center_based_not_supported", "اجرای آزمون مرکز-محور در این Patch فعال نشده است.");

        if (examData.Exam.Status != ExamStatus.Published)
            return Result<Guid>.Forbidden("exam.not_published", "آزمون هنوز منتشر نشده است.");

        var now = DateTime.UtcNow;
        if (now < examData.Exam.StartsAtUtc || now > examData.Exam.EndsAtUtc)
            return Result<Guid>.Forbidden("exam.out_of_window", "خارج از بازه مجاز آزمون هستید.");

        var enrolled = await _dbContext.Enrollments
            .AnyAsync(x => x.CourseOfferingId == examData.Id && x.StudentProfileId == studentProfileId.Value && x.IsActive, cancellationToken);

        if (!enrolled)
            return Result<Guid>.Forbidden("exam.not_enrolled", "دانشجو در این ارائه ثبت نشده است.");

        var attempts = await _dbContext.ExamSubmissions
            .CountAsync(x => x.ExamId == request.ExamId && x.StudentProfileId == studentProfileId.Value, cancellationToken);

        if (attempts >= examData.Exam.MaxAttemptsPerStudent)
            return Result<Guid>.Forbidden("exam.attempt_limit", "تعداد دفعات مجاز شرکت در آزمون به پایان رسیده است.");

        var activeSubmission = await _dbContext.ExamSubmissions
            .Where(x => x.ExamId == request.ExamId && x.StudentProfileId == studentProfileId.Value && x.Status == ExamSubmissionStatus.InProgress)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (activeSubmission.HasValue)
            return Result<Guid>.Success(activeSubmission.Value);

        var submission = ExamSubmission.Create(request.ExamId, studentProfileId.Value, attempts + 1);
        await _dbContext.AddAsync(submission, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(submission.Id);
    }
}