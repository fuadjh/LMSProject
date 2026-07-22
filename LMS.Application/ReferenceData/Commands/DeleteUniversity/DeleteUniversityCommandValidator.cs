using FluentValidation;

namespace Application.ReferenceData.Commands.DeleteUniversity;

public sealed class DeleteUniversityCommandValidator
    : AbstractValidator<DeleteUniversityCommand>
{
    public DeleteUniversityCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("شناسه دانشگاه معتبر نیست.");
    }
}