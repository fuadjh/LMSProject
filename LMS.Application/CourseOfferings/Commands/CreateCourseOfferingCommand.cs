using Application.Common.Results;
using MediatR;

namespace Application.CourseOfferings.Commands.CreateCourseOffering;

public sealed record CreateCourseOfferingCommand(
    Guid CourseId,
    Guid SemesterId) : IRequest<Result<Guid>>;