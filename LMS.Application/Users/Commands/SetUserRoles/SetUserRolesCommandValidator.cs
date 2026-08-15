using FluentValidation;

namespace Application.Users.Commands.SetUserRoles;

public sealed class SetUserRolesCommandValidator
    : AbstractValidator<SetUserRolesCommand>
{
    public SetUserRolesCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("شناسه کاربر الزامی است.");

        RuleFor(x => x.Roles)
            .NotNull()
            .WithMessage("لیست Roleها الزامی است.");

        RuleForEach(x => x.Roles)
            .NotEmpty()
            .MaximumLength(100);
    }
}