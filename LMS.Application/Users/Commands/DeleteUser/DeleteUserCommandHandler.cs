using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Common.Results;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.DeleteUser;

public sealed class DeleteUserCommandHandler
    : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IIdentityService _identityService;

    public DeleteUserCommandHandler(
        IApplicationDbContext dbContext,
        IIdentityService identityService)
    {
        _dbContext = dbContext;
        _identityService = identityService;
    }

    public async Task<Result> Handle(
        DeleteUserCommand request,
        CancellationToken cancellationToken)
    {
        var userProfile =
            await _dbContext.UserProfiles
                .Include(x => x.FacultyScopes)
                .Include(x => x.MajorScopes)
                .SingleOrDefaultAsync(
                    x => x.Id == request.UserProfileId,
                    cancellationToken);

        if (userProfile is null)
        {
            return Result.NotFound(
                "user_profile.not_found",
                "پروفایل کاربر یافت نشد.");
        }

        var studentProfile =
            await _dbContext.StudentProfiles.SingleOrDefaultAsync(
                x => x.UserProfileId == request.UserProfileId,
                cancellationToken);

        if (studentProfile is not null)
        {
            var hasEnrollment =
                await _dbContext.Enrollments.AnyAsync(
                    x => x.StudentProfileId == studentProfile.Id,
                    cancellationToken);

            if (hasEnrollment)
            {
                return Result.Conflict(
                    "user.has_enrollment",
                    "کاربر دارای سابقه ثبت‌نام در درس است و قابل حذف کامل نیست.");
            }

            var hasExamSubmission =
                await _dbContext.ExamSubmissions.AnyAsync(
                    x => x.StudentProfileId == studentProfile.Id,
                    cancellationToken);

            if (hasExamSubmission)
            {
                return Result.Conflict(
                    "user.has_exam_submission",
                    "کاربر دارای سابقه شرکت در آزمون است و قابل حذف کامل نیست.");
            }
        }

        var instructorProfile =
            await _dbContext.InstructorProfiles.SingleOrDefaultAsync(
                x => x.UserProfileId == request.UserProfileId,
                cancellationToken);

        if (instructorProfile is not null)
        {
            var hasCourseOffering =
                await _dbContext.CourseOfferings.AnyAsync(
                    x => x.InstructorProfileId == instructorProfile.Id,
                    cancellationToken);

            if (hasCourseOffering)
            {
                return Result.Conflict(
                    "user.has_course_offering",
                    "استاد به یک یا چند ارائه درس تخصیص داده شده است.");
            }

            var hasExam =
                await _dbContext.Exams.AnyAsync(
                    x => x.CreatedByInstructorProfileId ==
                         instructorProfile.Id,
                    cancellationToken);

            if (hasExam)
            {
                return Result.Conflict(
                    "user.has_created_exam",
                    "استاد دارای سابقه ایجاد آزمون است و قابل حذف کامل نیست.");
            }

            var hasQuestionBank =
                await _dbContext.QuestionBanks.AnyAsync(
                    x => x.InstructorProfileId == instructorProfile.Id,
                    cancellationToken);

            if (hasQuestionBank)
            {
                return Result.Conflict(
                    "user.has_question_bank",
                    "استاد دارای بانک سؤال است و قابل حذف کامل نیست.");
            }
        }

        await using var transaction =
            await _dbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            var authUserId = userProfile.AuthUserId;

            _dbContext.Remove(userProfile);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            await _identityService.DeleteUserAsync(
                authUserId,
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            return Result.Failure(
                "user.delete_failed",
                ex.Message);
        }
    }
}

public sealed class DeleteUserCommandValidator
    : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.UserProfileId)
            .NotEmpty();
    }
}