using Application.Common.Results;
using MediatR;

namespace Application.ReferenceData.Commands.DeleteUniversity;

public sealed record DeleteUniversityCommand(Guid Id) : IRequest<Result>;