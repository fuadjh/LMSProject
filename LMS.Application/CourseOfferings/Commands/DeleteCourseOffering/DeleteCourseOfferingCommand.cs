using Application.Common.Results;
using MediatR;

namespace Application.CourseOfferings.Commands.DeleteCourseOffering;

public sealed record DeleteCourseOfferingCommand(Guid Id)
    : IRequest<Result>;