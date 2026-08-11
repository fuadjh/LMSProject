using Application.Common.Results;
using Common.Enums;
using Common.Security;
using MediatR;

namespace Application.Users.Commands.UpdateUser;

public sealed class UpdateUserCommand : IRequest<Result<bool>>
{
    public Guid UserId { get; set; }

    public UserRoleType Role { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? LatinFirstName { get; set; }

    public string? LatinLastName { get; set; }

    public Gender Gender { get; set; }

    public string? ProfileImagePath { get; set; }

    public bool IsActive { get; set; }

    public string? StudentNumber { get; set; }

    public Guid? StudentMajorId { get; set; }

    public string? PersonnelCode { get; set; }

    public IReadOnlyCollection<Guid> FacultyScopeIds { get; set; } = [];

    public IReadOnlyCollection<Guid> MajorScopeIds { get; set; } = [];
}