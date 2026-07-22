using Application.Common.Results;
using Common.Enums;
using MediatR;

namespace Application.Courses.Commands.CreateCourse;

public sealed record CreateCourseCommand(
    Guid MajorId,
    string Title,
    string Code,
    int Units,
    CourseEnrollmentScope EnrollmentScope)
    : IRequest<Result<Guid>>;