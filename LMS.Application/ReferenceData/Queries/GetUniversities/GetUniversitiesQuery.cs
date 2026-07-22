using Application.Common.Models;
using Application.Common.Results;
using Common.Contracts.ReferenceData;
using MediatR;

namespace Application.ReferenceData.Queries.GetUniversities;

public sealed record GetUniversitiesQuery
    : PagedQuery,
      IRequest<Result<PagedResponse<UniversityLookupDto>>>;