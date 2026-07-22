using Application.Common.Models;
using Application.Common.Results;
using Common.Contracts.ReferenceData;
using MediatR;

namespace Application.ReferenceData.Queries.GetFaculties;

public sealed record GetFacultiesQuery
    : PagedQuery,
      IRequest<Result<PagedResponse<FacultyListItemDto>>>
{
    public Guid? UniversityId { get; init; }
}