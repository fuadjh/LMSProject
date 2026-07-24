using Application.Users.Commands.CreateEducationExpert;
using Application.Users.Commands.CreateInstructor;
using Application.Users.Commands.CreateStudent;
using Application.Users.Commands.SetUserRoles;
using Application.Users.Commands.SetUserScopes;
using FluentValidation;

namespace Application.Users.Commands;

public sealed class CreateStudentCommandValidator
    : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(200);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.StudentNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.MajorId).NotEmpty();
    }
}

public sealed class CreateInstructorCommandValidator
    : AbstractValidator<CreateInstructorCommand>
{
    public CreateInstructorCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(200);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.PersonnelCode)
            .NotEmpty()
            .MaximumLength(50);
    }
}

public sealed class CreateEducationExpertCommandValidator
    : AbstractValidator<CreateEducationExpertCommand>
{
    public CreateEducationExpertCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(200);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.EmployeeCode)
            .NotEmpty()
            .MaximumLength(50);
    }
}

public sealed class SetUserRolesCommandValidator
    : AbstractValidator<SetUserRolesCommand>
{
    public SetUserRolesCommandValidator()
    {
        RuleFor(x => x.AuthUserId).NotEmpty();
        RuleFor(x => x.Roles).NotNull();
    }
}

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