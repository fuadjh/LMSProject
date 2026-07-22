using Application.Common.Results;
using LMS.Application.Exams;
using MediatR;

namespace Application.Exams.Queries.GetExamSubmissionDetails;

public sealed record GetExamSubmissionDetailsQuery(Guid SubmissionId)
    : IRequest<Result<ExamSubmissionDetailsDto>>;