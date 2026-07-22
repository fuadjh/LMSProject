using Application.Common.Results;
using MediatR;

namespace Application.Semesters.Commands.UpdateSemester;

public sealed record UpdateSemesterCommand(
    Guid Id,
    string Title,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    bool IsActive)
    : IRequest<Result>;