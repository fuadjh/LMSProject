using Application.Common.Models;
using Application.Common.Results;
using Common.Contracts.Academic;
using MediatR;

namespace Application.CourseOfferings.Queries.SearchInstructors;

public sealed record SearchInstructorsQuery(string? Search)
    : IRequest<Result<IReadOnlyCollection<InstructorLookupDto>>>;