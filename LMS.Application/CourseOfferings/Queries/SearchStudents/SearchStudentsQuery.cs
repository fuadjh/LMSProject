using Application.Common.Models;
using Application.Common.Results;
using Common.Contracts.Academic;
using MediatR;

namespace Application.CourseOfferings.Queries.SearchStudents;

public sealed record SearchStudentsQuery(Guid? MajorId, string? Search)
    : IRequest<Result<IReadOnlyCollection<StudentLookupDto>>>;