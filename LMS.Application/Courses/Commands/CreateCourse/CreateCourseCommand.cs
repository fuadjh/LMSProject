using Application.Common.Results;
using MediatR;

namespace Application.Courses.Commands.CreateCourse;

public sealed record CreateCourseCommand(
    Guid MajorId,
    string Title,
    string Code,
    int Units) : IRequest<Result<Guid>>;