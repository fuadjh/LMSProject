using Application.Common.Results;
using MediatR;

namespace Application.CourseOfferings.Commands.UpdateCourseOffering;

public sealed record UpdateCourseOfferingCommand(
    Guid Id,
    string SectionCode,
    int Capacity,
    bool IsActive)
    : IRequest<Result>;