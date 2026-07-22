using Application.Abstractions.Read;
using Application.Common.Models;
using Application.Common.Results;
using Common.Contracts.Academic;
using MediatR;

namespace Application.CourseOfferings.Queries.GetCourseOfferingById;

public sealed class GetCourseOfferingByIdQueryHandler : IRequestHandler<GetCourseOfferingByIdQuery, Result<CourseOfferingDetailsDto>>
{
    private readonly IAcademicReadService _service;

    public GetCourseOfferingByIdQueryHandler(IAcademicReadService service)
    {
        _service = service;
    }

    public async Task<Result<CourseOfferingDetailsDto>> Handle(GetCourseOfferingByIdQuery request, CancellationToken cancellationToken)
    {
        var data = await _service.GetCourseOfferingAsync(request.OfferingId, cancellationToken);

        return data is null
            ? Result<CourseOfferingDetailsDto>.NotFound("offering.not_found", "ارائه درس یافت نشد.")
            : Result<CourseOfferingDetailsDto>.Success(data);
    }
}