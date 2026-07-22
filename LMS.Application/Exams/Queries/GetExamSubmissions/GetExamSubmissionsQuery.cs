using Application.Common.Results;
using LMS.Application.Exams;
using MediatR;

namespace Application.Exams.Queries.GetExamSubmissions;

public sealed record GetExamSubmissionsQuery(Guid ExamId)
    : IRequest<Result<IReadOnlyCollection<ExamSubmissionSummaryDto>>>;