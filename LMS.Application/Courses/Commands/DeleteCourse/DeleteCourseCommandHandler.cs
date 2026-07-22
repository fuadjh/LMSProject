using Application.Abstractions.Persistence;
using Application.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Courses.Commands.DeleteCourse;

public sealed class DeleteCourseCommandHandler
    : IRequestHandler<DeleteCourseCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteCourseCommandHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(
        DeleteCourseCommand request,
        CancellationToken cancellationToken)
    {
        var courseExists =
            await _dbContext.Courses.AnyAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (!courseExists)
        {
            return Result.NotFound(
                "course.not_found",
                "درس یافت نشد.");
        }

        var hasDependencies =
            await _dbContext.CourseOfferings.AnyAsync(
                x => x.CourseId == request.Id,
                cancellationToken) ||
            await _dbContext.QuestionBanks.AnyAsync(
                x => x.CourseId == request.Id,
                cancellationToken);

        if (hasDependencies)
        {
            return Result.Conflict(
                "course.has_dependencies",
                "به دلیل وجود ارائه یا بانک سؤال، حذف درس امکان‌پذیر نیست.");
        }

        await _dbContext.Courses
            .Where(x => x.Id == request.Id)
            .ExecuteDeleteAsync(cancellationToken);

        return Result.Success();
    }
}