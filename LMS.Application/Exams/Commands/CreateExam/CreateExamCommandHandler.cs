using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Common.Results;
using Domain.Entities.Exams;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Exams.Commands.CreateExam;

public sealed class CreateExamCommandHandler : IRequestHandler<CreateExamCommand, Result<Guid>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IApplicationDbContext _dbContext;

    public CreateExamCommandHandler(ICurrentUser currentUser, IApplicationDbContext dbContext)
    {
        _currentUser = currentUser;
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(CreateExamCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserProfileId.HasValue)
            return Result<Guid>.Unauthorized("auth.required", "کاربر احراز هویت نشده است.");

        var instructorProfileId = await _dbContext.InstructorProfiles
            .Where(x => x.UserProfileId == _currentUser.UserProfileId.Value)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (!instructorProfileId.HasValue)
            return Result<Guid>.Forbidden("instructor.required", "فقط استاد می‌تواند آزمون بسازد.");

        var offeringExists = await _dbContext.CourseOfferings
            .AnyAsync(x => x.Id == request.CourseOfferingId && x.InstructorProfileId == instructorProfileId.Value, cancellationToken);

        if (!offeringExists)
            return Result<Guid>.Forbidden("offering.access_denied", "این ارائه متعلق به استاد فعلی نیست.");

        var exam = Exam.Create(
            request.CourseOfferingId,
            instructorProfileId.Value,
            request.Title,
            request.DeliveryMode,
            request.StartsAtUtc,
            request.EndsAtUtc,
            request.DurationMinutes,
            request.MaxAttemptsPerStudent);

        await _dbContext.AddAsync(exam, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(exam.Id);
    }
}