using Application.Common.Models;
using Application.Common.Results;
using Common.Contracts.Academic;
using MediatR;

namespace Application.Courses.Queries.GetCourses;

public sealed record GetCoursesQuery(Guid? MajorId)
    : IRequest<Result<IReadOnlyCollection<CourseLookupDto>>>;