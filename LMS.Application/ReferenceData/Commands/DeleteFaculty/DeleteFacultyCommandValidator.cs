using FluentValidation;

namespace Application.ReferenceData.Commands.DeleteFaculty;

public sealed class DeleteFacultyCommandValidator
    : AbstractValidator<DeleteFacultyCommand>
{
    public DeleteFacultyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("شناسه دانشکده معتبر نیست.");
    }
}