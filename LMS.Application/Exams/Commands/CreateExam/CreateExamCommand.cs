using Application.Common.Results;
using Common.Enums;

using MediatR;

namespace Application.Exams.Commands.CreateExam;

public sealed record CreateExamCommand(
    Guid CourseOfferingId,
    string Title,
    ExamDeliveryMode DeliveryMode,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    int DurationMinutes,
    int MaxAttemptsPerStudent) : IRequest<Result<Guid>>;