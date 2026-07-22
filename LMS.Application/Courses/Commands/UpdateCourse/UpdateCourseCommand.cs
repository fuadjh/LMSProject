using Application.Common.Results;
using Common.Enums;
using MediatR;

namespace Application.Courses.Commands.UpdateCourse;

public sealed record UpdateCourseCommand(
    Guid Id,
    Guid MajorId,
    string Title,
    string Code,
    int Units,
    CourseEnrollmentScope EnrollmentScope,
    bool IsActive)
    : IRequest<Result>;