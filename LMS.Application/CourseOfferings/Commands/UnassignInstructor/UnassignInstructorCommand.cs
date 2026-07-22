using Application.Common.Results;
using MediatR;

namespace Application.CourseOfferings.Commands.UnassignInstructor;

public sealed record UnassignInstructorCommand(Guid CourseOfferingId) : IRequest<Result>;