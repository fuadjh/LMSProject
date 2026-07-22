using Application.Common.Models;
using Application.Common.Results;
using Common.Contracts.Academic;
using MediatR;

namespace Application.CourseOfferings.Queries.GetCourseOfferingById;

public sealed record GetCourseOfferingByIdQuery(Guid OfferingId)
    : IRequest<Result<CourseOfferingDetailsDto>>;