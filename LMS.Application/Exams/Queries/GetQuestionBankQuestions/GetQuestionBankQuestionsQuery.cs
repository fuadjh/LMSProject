using Application.Common.Results;
using LMS.Application.Exams;
using MediatR;

namespace Application.Exams.Queries.GetQuestionBankQuestions;

public sealed record GetQuestionBankQuestionsQuery(Guid CourseOfferingId)
    : IRequest<Result<IReadOnlyCollection<QuestionBankItemDto>>>;