using Application.Common.Results;
using MediatR;

namespace Application.Users.Commands.CreateInstructor;

public sealed record CreateInstructorCommand(
     string NationalCode,
    string UserName,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string PersonnelCode) : IRequest<Result<Guid>>
{

};