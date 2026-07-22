using FluentValidation;

namespace Application.ReferenceData.Commands.DeleteMajor;

public sealed class DeleteMajorCommandValidator
    : AbstractValidator<DeleteMajorCommand>
{
    public DeleteMajorCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("شناسه رشته معتبر نیست.");
    }
}
