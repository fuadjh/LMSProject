using Common.Enums;
using FluentValidation;

namespace Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandValidator
    : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.NationalCode)
            .NotEmpty()
            .Length(10);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Role)
            .NotEqual(UserRoleType.All);

        When(x => x.Role == UserRoleType.Student, () =>
        {
            RuleFor(x => x.StudentNumber)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.StudentMajorId)
                .NotNull()
                .NotEqual(Guid.Empty);
        });

        When(x => x.Role == UserRoleType.Instructor, () =>
        {
            RuleFor(x => x.PersonnelCode)
                .NotEmpty()
                .MaximumLength(50);
        });
    }
}