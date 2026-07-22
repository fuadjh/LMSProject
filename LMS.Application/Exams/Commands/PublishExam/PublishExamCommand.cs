using Application.Common.Results;
using MediatR;

namespace Application.Exams.Commands.PublishExam;

public sealed record PublishExamCommand(Guid ExamId) : IRequest<Result>;