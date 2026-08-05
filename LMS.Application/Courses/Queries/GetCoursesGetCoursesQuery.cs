using Application.Common.Models;
using Application.Common.Results;
using Common.Contracts.Academic;

using MediatR;

namespace Application.Courses.Queries.GetCourses;

public sealed record GetCoursesQuery
    : PagedQuery,
      IRequest<Result<PagedResponse<CourseListItemDto>>>
{
    public Guid? MajorId { get; init; }
}