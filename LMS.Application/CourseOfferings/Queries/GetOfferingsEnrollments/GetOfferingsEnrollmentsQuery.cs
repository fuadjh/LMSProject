using Application.Common.Models;
using Application.Common.Results;
using Common.Contracts.Academic;
using MediatR;

namespace Application.CourseOfferings.Queries.GetOfferingsEnrollments;

public sealed record GetOfferingsEnrollmentsQuery(Guid CourseOfferingId)
    : IRequest<Result<IReadOnlyCollection<EnrollmentListItemDto>>>; 