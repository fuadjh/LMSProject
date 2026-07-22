using Application.Abstractions.Read;
using Application.Common.Models;
using Application.Common.Results;
using Common.Contracts.Academic;
using MediatR;

namespace Application.Courses.Queries.GetCourses;

public sealed class GetCoursesQueryHandler : IRequestHandler<GetCoursesQuery, Result<IReadOnlyCollection<CourseLookupDto>>>
{
    private readonly IAcademicReadService _service;

    public GetCoursesQueryHandler(IAcademicReadService service)
    {
        _service = service;
    }

    public async Task<Result<IReadOnlyCollection<CourseLookupDto>>> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
    {
        var data = await _service.GetCoursesAsync(request.MajorId, cancellationToken);
        return Result<IReadOnlyCollection<CourseLookupDto>>.Success(data);
    }
}