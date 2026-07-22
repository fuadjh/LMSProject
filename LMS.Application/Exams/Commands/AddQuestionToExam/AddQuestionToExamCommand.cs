using Application.Common.Results;
using MediatR;

namespace Application.Exams.Commands.AddQuestionToExam;

public sealed record AddQuestionToExamCommand(
    Guid ExamId,
    Guid QuestionId,
    int Order,
    decimal Score) : IRequest<Result>;