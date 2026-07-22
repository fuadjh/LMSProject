using Application.Common.Results;
using MediatR;

namespace LMS.Application.CourseOfferings.Commands.CreateCourseOffering;

public sealed record CreateCourseOfferingCommand(
    Guid CourseId,
    Guid SemesterId,
    string SectionCode,
    int Capacity)
    : IRequest<Result<Guid>>;