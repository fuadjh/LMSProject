using Application.Common.Results;
using MediatR;

namespace Application.Semesters.Commands.DeleteSemester;

public sealed record DeleteSemesterCommand(Guid Id)
    : IRequest<Result>;