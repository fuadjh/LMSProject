using Application.Users.Commands.SetUserScopes;
using FluentValidation;

namespace LMS.Application.Users.Commands.SetUserScopes;

public sealed class SetUserScopesCommandValidator
    : AbstractValidator<SetUserScopesCommand>
{
    public SetUserScopesCommandValidator()
    {
        RuleFor(x => x.UserProfileId).NotEmpty();
        RuleFor(x => x.FacultyIds).NotNull();
        RuleFor(x => x.MajorIds).NotNull();
    }
}