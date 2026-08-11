using Application.Abstractions.Identity;
using Application.Abstractions.Persistence;
using Application.Common.Models;
using Application.Common.Results;
using Common.Contracts.Academic;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.CourseOfferings.Queries.GetOfferingsEnrollments;

public sealed class GetOfferingsEnrollmentsQueryHandler
    : IRequestHandler<
        GetOfferingsEnrollmentsQuery,
        Result<IReadOnlyCollection<EnrollmentListItemDto>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAccountService _userAccountService;

    public GetOfferingsEnrollmentsQueryHandler(
        IApplicationDbContext dbContext,
        IUserAccountService userAccountService)
    {
        _dbContext = dbContext;
        _userAccountService = userAccountService;
    }

    public async Task<Result<IReadOnlyCollection<EnrollmentListItemDto>>> Handle(
        GetOfferingsEnrollmentsQuery request,
        CancellationToken cancellationToken)
    {
        var enrollmentRows = await (
            from enrollment in _dbContext.Enrollments
            join student in _dbContext.StudentProfiles
                on enrollment.StudentProfileId equals student.Id
            where enrollment.CourseOfferingId ==
                  request.CourseOfferingId
            select new
            {
                EnrollmentId = enrollment.Id,
                StudentUserId = student.Id,
                student.StudentNumber,
                enrollment.IsActive,
                enrollment.EnrolledAtUtc
            })
            .ToListAsync(cancellationToken);

        if (enrollmentRows.Count == 0)
        {
            return Result<IReadOnlyCollection<EnrollmentListItemDto>>
                .Success(Array.Empty<EnrollmentListItemDto>());
        }

        var userIds = enrollmentRows
            .Select(row => row.StudentUserId)
            .Distinct()
            .ToArray();

        var users = new Dictionary<Guid, Application.Common.Models.UserAccountDto>();

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

        var data = enrollmentRows
            .Where(row => users.ContainsKey(row.StudentUserId))
            .Select(row =>
            {
                var user = users[row.StudentUserId];

                return new EnrollmentListItemDto(
                    row.EnrollmentId,
                    row.StudentUserId,
                    $"{user.FirstName} {user.LastName}".Trim(),
                    row.StudentNumber,
                    row.IsActive,
                    row.EnrolledAtUtc);
            })
            .OrderBy(item => item.StudentFullName)
            .ToArray();

        return Result<IReadOnlyCollection<EnrollmentListItemDto>>
            .Success(data);
    }
}