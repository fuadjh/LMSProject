using Application.Abstractions.Identity;
using Application.Abstractions.Persistence;
using Application.Common.Results;
using LMS.Application.Exams;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Exams.Queries.GetExamSubmissions;

public sealed class GetExamSubmissionsQueryHandler
    : IRequestHandler<
        GetExamSubmissionsQuery,
        Result<IReadOnlyCollection<ExamSubmissionSummaryDto>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAccountService _userAccountService;

    public GetExamSubmissionsQueryHandler(
        IApplicationDbContext dbContext,
        IUserAccountService userAccountService)
    {
        _dbContext = dbContext;
        _userAccountService = userAccountService;
    }

    public async Task<Result<IReadOnlyCollection<ExamSubmissionSummaryDto>>> Handle(
        GetExamSubmissionsQuery request,
        CancellationToken cancellationToken)
    {
        var submissionRows = await (
            from submission in _dbContext.ExamSubmissions
            join student in _dbContext.StudentProfiles
                on submission.StudentProfileId equals student.Id
            where submission.ExamId == request.ExamId
            select new
            {
                SubmissionId = submission.Id,
                StudentUserId = student.Id,
                student.StudentNumber,
                submission.AttemptNumber,
                submission.StartedAtUtc,
                submission.SubmittedAtUtc,
                submission.TotalScore
            })
            .OrderByDescending(row => row.AttemptNumber)
            .ToListAsync(cancellationToken);

        if (submissionRows.Count == 0)
        {
            return Result<IReadOnlyCollection<ExamSubmissionSummaryDto>>
                .Success(Array.Empty<ExamSubmissionSummaryDto>());
        }

        var userIds = submissionRows
            .Select(row => row.StudentUserId)
            .Distinct()
            .ToArray();

        var users =
            new Dictionary<Guid, Application.Common.Models.UserAccountDto>();

        foreach (var userId in userIds)
        {
            var user = await _userAccountService.FindByIdAsync(
                userId,
                cancellationToken);

            if (user is not null)
            {
                users[userId] = user;
            }
        }

        var items = submissionRows
            .Where(row => users.ContainsKey(row.StudentUserId))
            .Select(row =>
            {
                var user = users[row.StudentUserId];

                return new ExamSubmissionSummaryDto(
                    row.SubmissionId,
                    $"{user.FirstName} {user.LastName}".Trim(),
                    row.StudentNumber,
                    row.AttemptNumber,
                    row.StartedAtUtc,
                    row.SubmittedAtUtc,
                    row.TotalScore);
            })
            .ToArray();

        return Result<IReadOnlyCollection<ExamSubmissionSummaryDto>>
            .Success(items);
    }
}