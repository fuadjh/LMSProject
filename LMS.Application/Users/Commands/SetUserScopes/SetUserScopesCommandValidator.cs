using FluentValidation;

namespace Application.Users.Commands.SetUserScopes;

public sealed class SetUserScopesCommandValidator
    : AbstractValidator<SetUserScopesCommand>
{
    public SetUserScopesCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("شناسه کاربر الزامی است.");

        RuleFor(x => x.FacultyIds)
            .NotNull();

        RuleFor(x => x.MajorIds)
            .NotNull();

        RuleForEach(x => x.FacultyIds)
            .NotEmpty();

        RuleForEach(x => x.MajorIds)
            .NotEmpty();
    }
}