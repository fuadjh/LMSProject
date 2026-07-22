using Application.Abstractions.Read;
using Application.Common.Models;
using Application.Common.Results;
using Common.Contracts.Academic;
using MediatR;

namespace Application.Semesters.Queries.GetSemesters;

public sealed class GetSemestersQueryHandler : IRequestHandler<GetSemestersQuery, Result<IReadOnlyCollection<SemesterLookupDto>>>
{
    private readonly IAcademicReadService _service;

    public GetSemestersQueryHandler(IAcademicReadService service)
    {
        _service = service;
    }

    public async Task<Result<IReadOnlyCollection<SemesterLookupDto>>> Handle(GetSemestersQuery request, CancellationToken cancellationToken)
    {
        var data = await _service.GetSemestersAsync(cancellationToken);
        return Result<IReadOnlyCollection<SemesterLookupDto>>.Success(data);
    }
}