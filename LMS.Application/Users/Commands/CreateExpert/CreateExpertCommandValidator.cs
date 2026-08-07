using Common.Validation;
using FluentValidation;

namespace Application.Users.Commands.CreateExpert;

public sealed class CreateExpertCommandValidator
    : AbstractValidator<CreateExpertCommand>
{
    public CreateExpertCommandValidator()
    {
        RuleFor(command => command.UserName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(command => command.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.NationalCode)
            .Must(IranianIdentityNormalizer.IsValidNationalCode)
            .WithMessage("کد ملی معتبر نیست.");

        RuleFor(command => command.PhoneNumber)
            .Must(IranianIdentityNormalizer.IsValidMobile)
            .WithMessage("شماره موبایل معتبر نیست.");

        RuleFor(command => command.Email)
            .EmailAddress()
            .When(command => !string.IsNullOrWhiteSpace(command.Email));

        RuleFor(command => command.Password)
            .NotEmpty()
            .MinimumLength(6);
    }
}