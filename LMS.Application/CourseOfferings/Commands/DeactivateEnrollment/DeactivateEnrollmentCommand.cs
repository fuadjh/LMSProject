using Application.Common.Results;
using MediatR;

namespace Application.CourseOfferings.Commands.DeactivateEnrollment;

public sealed record DeactivateEnrollmentCommand(Guid EnrollmentId) : IRequest<Result>;