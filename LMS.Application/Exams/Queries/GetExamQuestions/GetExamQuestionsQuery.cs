using Application.Common.Results;
using LMS.Application.Exams;
using MediatR;

namespace Application.Exams.Queries.GetExamQuestions;

public sealed record GetExamQuestionsQuery(Guid ExamId)
    : IRequest<Result<IReadOnlyCollection<ExamQuestionManageDto>>>;