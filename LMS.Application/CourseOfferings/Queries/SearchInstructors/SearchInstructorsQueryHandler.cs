using Application.Abstractions.Read;
using Application.Common.Models;
using Application.Common.Results;
using Common.Contracts.Academic;
using MediatR;

namespace Application.CourseOfferings.Queries.SearchInstructors;

public sealed class SearchInstructorsQueryHandler : IRequestHandler<SearchInstructorsQuery, Result<IReadOnlyCollection<InstructorLookupDto>>>
{
    private readonly IAcademicReadService _service;

    public SearchInstructorsQueryHandler(IAcademicReadService service)
    {
        _service = service;
    }

    public async Task<Result<IReadOnlyCollection<InstructorLookupDto>>> Handle(SearchInstructorsQuery request, CancellationToken cancellationToken)
    {
        var data = await _service.SearchInstructorsAsync(request.Search, cancellationToken);
        return Result<IReadOnlyCollection<InstructorLookupDto>>.Success(data);
    }
}