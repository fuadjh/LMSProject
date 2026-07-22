using Application.Common.Results;
using MediatR;

namespace Application.Exams.Commands.GradeEssayAnswer;

public sealed record GradeEssayAnswerCommand(Guid AnswerId, decimal AwardedScore)
    : IRequest<Result>;