using FluentValidation;

namespace Application.ReferenceData.Commands.UpdateMajor;

public sealed class UpdateMajorCommandValidator
    : AbstractValidator<UpdateMajorCommand>
{
    public UpdateMajorCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("شناسه رشته معتبر نیست.");

        RuleFor(x => x.FacultyId)
            .NotEmpty()
            .WithMessage("انتخاب دانشکده الزامی است.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("عنوان رشته الزامی است.")
            .MaximumLength(200)
            .WithMessage("عنوان رشته نمی‌تواند بیشتر از ۲۰۰ کاراکتر باشد.");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("کد رشته الزامی است.")
            .MaximumLength(50)
            .WithMessage("کد رشته نمی‌تواند بیشتر از ۵۰ کاراکتر باشد.");
    }
}
