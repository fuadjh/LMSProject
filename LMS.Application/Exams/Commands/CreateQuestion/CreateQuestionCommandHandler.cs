using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Common.Results;
using Common.Enums;
using Domain.Entities.Exams;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Exams.Commands.CreateQuestion;

public sealed class CreateQuestionCommandHandler : IRequestHandler<CreateQuestionCommand, Result<Guid>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IApplicationDbContext _dbContext;

    public CreateQuestionCommandHandler(ICurrentUser currentUser, IApplicationDbContext dbContext)
    {
        _currentUser = currentUser;
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(CreateQuestionCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            return Result<Guid>.Unauthorized("auth.required", "کاربر احراز هویت نشده است.");

        var instructorProfileId = await _dbContext.InstructorProfiles
            .Where(x => x.Id == _currentUser.UserId.Value)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (!instructorProfileId.HasValue)
            return Result<Guid>.Forbidden("instructor.required", "فقط استاد می‌تواند بانک سؤال را مدیریت کند.");

        var offering = await _dbContext.CourseOfferings
            .Where(x => x.Id == request.CourseOfferingId && x.InstructorProfileId == instructorProfileId.Value)
            .Select(x => new { x.CourseId })
            .SingleOrDefaultAsync(cancellationToken);

        if (offering is null)
            return Result<Guid>.Forbidden("offering.access_denied", "این ارائه متعلق به استاد فعلی نیست.");

        var bank = await _dbContext.QuestionBanks
            .SingleOrDefaultAsync(x => x.CourseId == offering.CourseId && x.InstructorProfileId == instructorProfileId.Value, cancellationToken);

        if (bank is null)
        {
            bank = QuestionBank.Create(offering.CourseId, instructorProfileId.Value);
            await _dbContext.AddAsync(bank, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        await using var tx = await _dbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            var question = Question.Create(
                bank.Id,
                request.Title,
                request.Body,
                request.Type,
                request.Difficulty,
                request.EvaluationDomain,
                request.SourceType,
                request.SourceDescription,
                request.SuggestedScore,
                request.AttachmentFileName,
                request.AttachmentPath);

            await _dbContext.AddAsync(question, cancellationToken);

            if (request.Type == QuestionType.MultipleChoice)
            {
                foreach (var option in request.Options.OrderBy(x => x.Order))
                {
                    var entity = QuestionOption.Create(question.Id, option.Text, option.Order, option.IsCorrect);
                    await _dbContext.AddAsync(entity, cancellationToken);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return Result<Guid>.Success(question.Id);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return Result<Guid>.Failure("question.create_failed", ex.Message);
        }
    }
}