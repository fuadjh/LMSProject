using Common.Enums;
using FluentValidation;

namespace Application.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandValidator
    : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.Role)
            .NotEqual(UserRoleType.All);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        When(x => x.Role == UserRoleType.Student, () =>
        {
            RuleFor(x => x.StudentNumber).NotEmpty();
            RuleFor(x => x.StudentMajorId)
                .NotNull()
                .NotEqual(Guid.Empty);
        });

        When(x => x.Role == UserRoleType.Instructor, () =>
        {
            RuleFor(x => x.PersonnelCode).NotEmpty();
        });
    }
}