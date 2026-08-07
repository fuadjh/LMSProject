using Application.Abstractions.Persistence;
using Application.Common.Models;
using Application.Common.Results;
using Common.Contracts.Academic;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.CourseOfferings.Queries.GetOfferingsEnrollments;

public sealed class GetOfferingsEnrollmentsQueryHandler : IRequestHandler<GetOfferingsEnrollmentsQuery, Result<IReadOnlyCollection<EnrollmentListItemDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetOfferingsEnrollmentsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyCollection<EnrollmentListItemDto>>> Handle(GetOfferingsEnrollmentsQuery request, CancellationToken cancellationToken)
    {
        var data = await (
            from e in _dbContext.Enrollments
            join s in _dbContext.StudentProfiles on e.StudentProfileId equals s.Id
            join p in _dbIUserAccountService on s.UserId equals p.Id
            where e.CourseOfferingId == request.CourseOfferingId
            orderby p.FirstName, p.LastName
            select new EnrollmentListItemDto(
                e.Id,
                s.Id,
                p.FirstName + " " + p.LastName,
                s.StudentNumber,
                e.IsActive,
                e.EnrolledAtUtc))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyCollection<EnrollmentListItemDto>>.Success(data);
    }
}