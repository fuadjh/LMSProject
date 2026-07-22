using Application.Common.Models;
using Application.Common.Results;
using Common.Contracts.ReferenceData;
using MediatR;

namespace Application.ReferenceData.Queries.GetMajors;

public sealed record GetMajorsQuery
    : PagedQuery,
      IRequest<Result<PagedResponse<MajorListItemDto>>>
{
    public Guid? FacultyId { get; init; }
}
