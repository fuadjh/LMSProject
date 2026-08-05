using Application.Users.Commands.SetUserRoles;
using FluentValidation;

namespace LMS.Application.Users.Commands.SetUserRoles;

public sealed class SetUserRolesCommandValidator
    : AbstractValidator<SetUserRolesCommand>
{
    public SetUserRolesCommandValidator()
    {
        RuleFor(x => x.AuthUserId).NotEmpty();
        RuleFor(x => x.Roles).NotNull();
    }
}
