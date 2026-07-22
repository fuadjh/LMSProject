using Application.Common.Results;
using MediatR;

namespace Application.CourseOfferings.Commands.EnrollStudent;

public sealed record EnrollStudentCommand(
    Guid CourseOfferingId,
    Guid StudentProfileId) : IRequest<Result>;