using Application.Common.Results;
using MediatR;

namespace Application.ReferenceData.Commands.CreateFaculty;

public sealed record CreateFacultyCommand(
    Guid UniversityId,
    string Title,
    string Code) : IRequest<Result<Guid>>;