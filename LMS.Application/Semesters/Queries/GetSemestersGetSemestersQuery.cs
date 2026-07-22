using Application.Common.Models;
using Application.Common.Results;
using Common.Contracts.Academic;
using MediatR;

namespace Application.Semesters.Queries.GetSemesters;

public sealed record GetSemestersQuery : IRequest<Result<IReadOnlyCollection<SemesterLookupDto>>>;