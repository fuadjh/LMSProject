using Application.Abstractions.Read;
using Application.Common.Models;
using Application.Common.Results;
using Common.Contracts.Academic;
using MediatR;

namespace Application.CourseOfferings.Queries.SearchStudents;

public sealed class SearchStudentsQueryHandler : IRequestHandler<SearchStudentsQuery, Result<IReadOnlyCollection<StudentLookupDto>>>
{
    private readonly IAcademicReadService _service;

    public SearchStudentsQueryHandler(IAcademicReadService service)
    {
        _service = service;
    }

    public async Task<Result<IReadOnlyCollection<StudentLookupDto>>> Handle(SearchStudentsQuery request, CancellationToken cancellationToken)
    {
        var data = await _service.SearchStudentsAsync(request.MajorId, request.Search, cancellationToken);
        return Result<IReadOnlyCollection<StudentLookupDto>>.Success(data);
    }
}