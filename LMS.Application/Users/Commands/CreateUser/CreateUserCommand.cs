using Application.Common.Results;
using Common.Enums;
using Common.Security;
using MediatR;

namespace Application.Users.Commands.CreateUser;

public sealed class CreateUserCommand : IRequest<Result<Guid>>
{
    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string NationalCode { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? LatinFirstName { get; set; }

    public string? LatinLastName { get; set; }

    public Gender Gender { get; set; }

    public string? ProfileImagePath { get; set; }

    public UserRoleType Role { get; set; }

    public string? StudentNumber { get; set; }

    public Guid? StudentMajorId { get; set; }

    public string? PersonnelCode { get; set; }

    public IReadOnlyCollection<Guid> FacultyScopeIds { get; set; } = [];

    public IReadOnlyCollection<Guid> MajorScopeIds { get; set; } = [];
}