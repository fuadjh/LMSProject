using Application.Common.Models;
using Common.Enums;
using MediatR;

namespace Application.Users.Commands.CreateExpert;

public sealed record CreateExpertCommand(
    string UserName,
    string FirstName,
    string LastName,
    string NationalCode,
    string PhoneNumber,
    string? Email,
    string? LatinFirstName,
    string? LatinLastName,
    Gender Gender,
    string? ProfileImagePath,
    string Password)
    : IRequest<IdentityOperationResult>;