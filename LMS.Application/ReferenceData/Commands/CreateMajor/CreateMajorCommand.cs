using Application.Common.Results;
using MediatR;

namespace Application.ReferenceData.Commands.CreateMajor;

public sealed record CreateMajorCommand(
    Guid FacultyId,
    string Title,
    string Code) : IRequest<Result<Guid>>;