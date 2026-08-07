using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Common.Results;
using LMS.Application.Exams;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Exams.Queries.GetQuestionBankQuestions;

public sealed class GetQuestionBankQuestionsQueryHandler : IRequestHandler<GetQuestionBankQuestionsQuery, Result<IReadOnlyCollection<QuestionBankItemDto>>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IApplicationDbContext _dbContext;

    public GetQuestionBankQuestionsQueryHandler(ICurrentUser currentUser, IApplicationDbContext dbContext)
    {
        _currentUser = currentUser;
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyCollection<QuestionBankItemDto>>> Handle(GetQuestionBankQuestionsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            return Result<IReadOnlyCollection<QuestionBankItemDto>>.Unauthorized("auth.required", "کاربر احراز هویت نشده است.");

        var instructorProfileId = await _dbContext.InstructorProfiles
            .Where(x => x.UserId == _currentUser.UserId.Value)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (!instructorProfileId.HasValue)
            return Result<IReadOnlyCollection<QuestionBankItemDto>>.Forbidden("instructor.required", "فقط استاد مجاز است.");

        var courseId = await _dbContext.CourseOfferings
            .Where(x => x.Id == request.CourseOfferingId && x.InstructorProfileId == instructorProfileId.Value)
            .Select(x => (Guid?)x.CourseId)
            .SingleOrDefaultAsync(cancellationToken);

        if (!courseId.HasValue)
            return Result<IReadOnlyCollection<QuestionBankItemDto>>.Forbidden("offering.access_denied", "این ارائه متعلق به استاد فعلی نیست.");

        var bankId = await _dbContext.QuestionBanks
            .Where(x => x.CourseId == courseId.Value && x.InstructorProfileId == instructorProfileId.Value && x.IsActive)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (!bankId.HasValue)
            return Result<IReadOnlyCollection<QuestionBankItemDto>>.Success(Array.Empty<QuestionBankItemDto>());

        var questions = await _dbContext.Questions
            .Where(x => x.QuestionBankId == bankId.Value && x.IsActive)
            .OrderBy(x => x.Title)
            .Select(x => new QuestionBankItemDto(
                x.Id,
                x.Title,
                x.Body,
                x.Type,
                x.Difficulty,
                x.EvaluationDomain,
                x.SourceType,
                x.SourceDescription,
                x.SuggestedScore,
                x.AttachmentFileName,
                x.AttachmentPath,
                Array.Empty<QuestionOptionDto>()))
            .ToListAsync(cancellationToken);

        var ids = questions.Select(x => x.QuestionId).ToArray();

        var optionMap = await _dbContext.QuestionOptions
            .Where(x => ids.Contains(x.QuestionId))
            .OrderBy(x => x.Order)
            .Select(x => new { x.QuestionId, Item = new QuestionOptionDto(x.Id, x.Text, x.Order, x.IsCorrect) })
            .ToListAsync(cancellationToken);

        var result = questions
            .Select(q => q with
            {
                Options = optionMap.Where(x => x.QuestionId == q.QuestionId).Select(x => x.Item).ToArray()
            })
            .ToArray();

        return Result<IReadOnlyCollection<QuestionBankItemDto>>.Success(result);
    }
}