using Application.Common.Results;
using LMS.Application.Exams;
using MediatR;

namespace Application.Exams.Queries.GetExamsByOffering;

public sealed record GetExamsByOfferingQuery(Guid CourseOfferingId)
    : IRequest<Result<IReadOnlyCollection<ExamLookupDto>>>;