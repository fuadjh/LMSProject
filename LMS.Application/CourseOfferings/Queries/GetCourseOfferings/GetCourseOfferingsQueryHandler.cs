using Application.Abstractions.Read;
using Application.Common.Models;
using Application.Common.Results;
using Common.Contracts.Academic;
using MediatR;

namespace Application.CourseOfferings.Queries.GetCourseOfferings;

public sealed class GetCourseOfferingsQueryHandler : IRequestHandler<GetCourseOfferingsQuery, Result<IReadOnlyCollection<CourseOfferingLookupDto>>>
{
    private readonly IAcademicReadService _service;

    public GetCourseOfferingsQueryHandler(IAcademicReadService service)
    {
        _service = service;
    }

    public async Task<Result<IReadOnlyCollection<CourseOfferingLookupDto>>> Handle(GetCourseOfferingsQuery request, CancellationToken cancellationToken)
    {
        var data = await _service.GetCourseOfferingsAsync(request.SemesterId, cancellationToken);
        return Result<IReadOnlyCollection<CourseOfferingLookupDto>>.Success(data);
    }
}