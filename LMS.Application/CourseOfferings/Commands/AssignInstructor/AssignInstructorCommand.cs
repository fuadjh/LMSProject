using Application.Common.Results;
using MediatR;

namespace Application.CourseOfferings.Commands.AssignInstructor;

public sealed record AssignInstructorCommand(
    Guid CourseOfferingId,
    Guid InstructorProfileId) : IRequest<Result>;