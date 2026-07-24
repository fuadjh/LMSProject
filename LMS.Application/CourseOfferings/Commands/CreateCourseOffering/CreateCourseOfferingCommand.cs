using Application.Common.Results;
using MediatR;

namespace Application.CourseOfferings.Commands.CreateCourseOffering;

public sealed record CreateCourseOfferingCommand(
    Guid CourseId,
    Guid SemesterId,
    string SectionCode,
    int Capacity,
    DateTime? StartsAtUtc,
    DateTime? EndsAtUtc) : IRequest<Result<Guid>>;