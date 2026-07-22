using Application.Common.Results;
using MediatR;

namespace Application.ReferenceData.Commands.DeleteFaculty;

public sealed record DeleteFacultyCommand(Guid Id) : IRequest<Result>;