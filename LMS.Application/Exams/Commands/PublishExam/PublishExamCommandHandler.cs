using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Common.Results;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Exams.Commands.PublishExam;

public sealed class PublishExamCommandHandler : IRequestHandler<PublishExamCommand, Result>
{
    private readonly ICurrentUser _currentUser;
    private readonly IApplicationDbContext _dbContext;

    public PublishExamCommandHandler(ICurrentUser currentUser, IApplicationDbContext dbContext)
    {
        _currentUser = currentUser;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(PublishExamCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            return Result.Unauthorized("auth.required", "کاربر احراز هویت نشده است.");

        var instructorProfileId = await _dbContext.InstructorProfiles
            .Where(x => x.UserId == _currentUser.UserId.Value)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (!instructorProfileId.HasValue)
            return Result.Forbidden("instructor.required", "فقط استاد مجاز است.");

        var exam = await _dbContext.Exams
            .SingleOrDefaultAsync(x => x.Id == request.ExamId && x.CreatedByInstructorProfileId == instructorProfileId.Value, cancellationToken);

        if (exam is null)
            return Result.NotFound("exam.not_found", "آزمون یافت نشد.");

        var questionCount = await _dbContext.ExamQuestions
            .CountAsync(x => x.ExamId == request.ExamId, cancellationToken);

        if (questionCount == 0)
            return Result.Invalid(Error.Validation("questions", "آزمون باید حداقل یک سوال داشته باشد."));

        exam.Publish();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}