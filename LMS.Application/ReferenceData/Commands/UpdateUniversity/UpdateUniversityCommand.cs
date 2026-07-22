using Application.Common.Results;
using MediatR;

namespace Application.ReferenceData.Commands.UpdateUniversity;

public sealed record UpdateUniversityCommand(
    Guid Id,
    string Title,
    string Code,
    bool IsActive) : IRequest<Result>;