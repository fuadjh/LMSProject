using FluentValidation;

namespace Application.ReferenceData.Commands.UpdateFaculty;

public sealed class UpdateFacultyCommandValidator
    : AbstractValidator<UpdateFacultyCommand>
{
    public UpdateFacultyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("شناسه دانشکده معتبر نیست.");

        RuleFor(x => x.UniversityId)
            .NotEmpty()
            .WithMessage("انتخاب دانشگاه الزامی است.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("عنوان دانشکده الزامی است.")
            .MaximumLength(200)
            .WithMessage("عنوان دانشکده نمی‌تواند بیشتر از ۲۰۰ کاراکتر باشد.");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("کد دانشکده الزامی است.")
            .MaximumLength(50)
            .WithMessage("کد دانشکده نمی‌تواند بیشتر از ۵۰ کاراکتر باشد.");
    }
}