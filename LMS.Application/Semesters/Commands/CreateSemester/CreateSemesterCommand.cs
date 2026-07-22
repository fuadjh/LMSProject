using Application.Common.Results;
using MediatR;

namespace Application.Semesters.Commands.CreateSemester;

public sealed record CreateSemesterCommand(
    string Title,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc) : IRequest<Result<Guid>>;