using Application.Common.Models;
using Application.Common.Results;
using Common.Contracts.Academic;
using MediatR;

namespace Application.CourseOfferings.Queries.GetCourseOfferings;

public sealed record GetCourseOfferingsQuery(Guid? SemesterId)
    : IRequest<Result<IReadOnlyCollection<CourseOfferingLookupDto>>>;