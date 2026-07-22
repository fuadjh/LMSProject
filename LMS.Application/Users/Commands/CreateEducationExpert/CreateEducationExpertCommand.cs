using Application.Common.Results;
using MediatR;

namespace Application.Users.Commands.CreateEducationExpert;

public sealed record CreateEducationExpertCommand(
    string UserName,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string EmployeeCode) : IRequest<Result<Guid>>;