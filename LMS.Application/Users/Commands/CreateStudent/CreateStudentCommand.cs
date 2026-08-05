using Application.Common.Results;
using MediatR;

public sealed record CreateStudentCommand(
    string NationalCode,
    string UserName,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string StudentNumber,
    Guid MajorId) : IRequest<Result<Guid>>;