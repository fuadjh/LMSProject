using Application.Common.Results;
using MediatR;

namespace Application.ReferenceData.Commands.UpdateMajor;

public sealed record UpdateMajorCommand(
    Guid Id,
    Guid FacultyId,
    string Title,
    string Code,
    bool IsActive) : IRequest<Result>;
