using Application.Common.Results;
using MediatR;

namespace Application.ReferenceData.Commands.CreateUniversity;

public sealed record CreateUniversityCommand(string Title, string Code) : IRequest<Result<Guid>>;