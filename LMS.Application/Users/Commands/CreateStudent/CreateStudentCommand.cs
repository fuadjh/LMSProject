using Application.Common.Results;
using MediatR;

namespace Application.Users.Commands.CreateStudent;

public sealed record CreateStudentCommand(
    string UserName,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string StudentNumber,
    Guid MajorId):IRequest<Result<Guid>>;