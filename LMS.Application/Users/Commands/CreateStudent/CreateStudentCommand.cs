using Application.Common.Models;
using Common.Enums;
using MediatR;

namespace Application.Users.Commands.CreateStudent;

public sealed record CreateStudentCommand(
    string FirstName,
    string LastName,
    string NationalCode,
    string PhoneNumber,
    string? Email,
    string? LatinFirstName,
    string? LatinLastName,
    Gender Gender,
    string? ProfileImagePath,
    string StudentNumber,
    Guid MajorId,
    string Password)
    : IRequest<IdentityOperationResult>;