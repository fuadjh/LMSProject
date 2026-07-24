using Application.Common.Results;
using Common.Contracts.Learning;
using MediatR;

namespace Application.Learning.Queries.GetOfferingLearningContent;

public sealed record GetOfferingLearningContentQuery(
    Guid CourseOfferingId)
    : IRequest<Result<LearningContentDto>>;
