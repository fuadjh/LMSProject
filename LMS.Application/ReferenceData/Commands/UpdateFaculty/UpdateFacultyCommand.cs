using Application.Common.Results;
using MediatR;

namespace Application.ReferenceData.Commands.UpdateFaculty;

public sealed record UpdateFacultyCommand(
    Guid Id,
    Guid UniversityId,
    string Title,
    string Code,
    bool IsActive) : IRequest<Result>;