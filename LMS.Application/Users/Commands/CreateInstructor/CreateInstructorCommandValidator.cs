using Domain.Entities.Users;
using FluentValidation;

namespace Application.Users.Commands.CreateInstructor;

public sealed class CreateInstructorCommandValidator
    : AbstractValidator<CreateInstructorCommand>
{
    public CreateInstructorCommandValidator()
    {
        RuleFor(x => x.NationalCode)
            .NotEmpty()
            .WithMessage("کد ملی الزامی است.")
            .Must(UserProfile.IsValidNationalCode)
            .WithMessage("کد ملی معتبر نیست.");

        RuleFor(x => x.UserName)
            .MaximumLength(100)
            .WithMessage("نام کاربری نمی‌تواند بیشتر از 100 کاراکتر باشد.");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("فرمت ایمیل معتبر نیست.")
            .MaximumLength(200)
            .WithMessage("ایمیل نمی‌تواند بیشتر از 200 کاراکتر باشد.");

        RuleFor(x => x.Password)
            .MinimumLength(6)
            .When(x => !string.IsNullOrWhiteSpace(x.Password))
            .WithMessage("رمز عبور باید حداقل 6 کاراکتر باشد.")
            .MaximumLength(200)
            .WithMessage("رمز عبور نمی‌تواند بیشتر از 200 کاراکتر باشد.");

        RuleFor(x => x.FirstName)
            .MaximumLength(100)
            .WithMessage("نام نمی‌تواند بیشتر از 100 کاراکتر باشد.");

        RuleFor(x => x.LastName)
            .MaximumLength(100)
            .WithMessage("نام خانوادگی نمی‌تواند بیشتر از 100 کاراکتر باشد.");

        RuleFor(x => x.PersonnelCode)
            .NotEmpty()
            .WithMessage("کد پرسنلی الزامی است.")
            .MaximumLength(50)
            .WithMessage("کد پرسنلی نمی‌تواند بیشتر از 50 کاراکتر باشد.");
    }
}