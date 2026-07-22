using Application.Common.Results;
using MediatR;

namespace Application.Exams.Commands.StartExamAttempt;

public sealed record StartExamAttemptCommand(Guid ExamId) : IRequest<Result<Guid>>;