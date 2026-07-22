using FluentValidation;

namespace Application.ReferenceData.Commands.UpdateUniversity;

public sealed class UpdateUniversityCommandValidator
    : AbstractValidator<UpdateUniversityCommand>
{
    public UpdateUniversityCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("شناسه دانشگاه معتبر نیست.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("عنوان دانشگاه الزامی و معتبر نیست.");

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("کد دانشگاه الزامی و معتبر نیست.");
    }
}