using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Common.Results;
using Domain.Entities.Exams;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Exams.Commands.AddQuestionToExam;

public sealed class AddQuestionToExamCommandHandler : IRequestHandler<AddQuestionToExamCommand, Result>
{
    private readonly ICurrentUser _currentUser;
    private readonly IApplicationDbContext _dbContext;

    public AddQuestionToExamCommandHandler(ICurrentUser currentUser, IApplicationDbContext dbContext)
    {
        _currentUser = currentUser;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(AddQuestionToExamCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserProfileId.HasValue)
            return Result.Unauthorized("auth.required", "کاربر احراز هویت نشده است.");

        var instructorProfileId = await _dbContext.InstructorProfiles
            .Where(x => x.UserProfileId == _currentUser.UserProfileId.Value)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (!instructorProfileId.HasValue)
            return Result.Forbidden("instructor.required", "فقط استاد مجاز است.");

        var examData = await (
            from e in _dbContext.Exams
            join o in _dbContext.CourseOfferings on e.CourseOfferingId equals o.Id
            where e.Id == request.ExamId && o.InstructorProfileId == instructorProfileId.Value
            select new { Exam = e, o.CourseId })
            .SingleOrDefaultAsync(cancellationToken);

        if (examData is null)
            return Result.NotFound("exam.not_found", "آزمون یافت نشد.");

        var questionOk = await (
            from q in _dbContext.Questions
            join b in _dbContext.QuestionBanks on q.QuestionBankId equals b.Id
            where q.Id == request.QuestionId &&
                  b.CourseId == examData.CourseId &&
                  b.InstructorProfileId == instructorProfileId.Value
            select q.Id)
            .AnyAsync(cancellationToken);

        if (!questionOk)
            return Result.Forbidden("question.not_allowed", "این سوال متعلق به بانک سوال همین استاد/درس نیست.");

        var exists = await _dbContext.ExamQuestions
            .AnyAsync(x => x.ExamId == request.ExamId && x.QuestionId == request.QuestionId, cancellationToken);

        if (exists)
            return Result.Conflict("exam_question.exists", "این سوال قبلاً به آزمون اضافه شده است.");

        var entity = ExamQuestion.Create(request.ExamId, request.QuestionId, request.Order, request.Score);

        await _dbContext.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}