using Application.Common.Results;
using MediatR;

namespace Application.Courses.Commands.DeleteCourse;

public sealed record DeleteCourseCommand(Guid Id)
    : IRequest<Result>;