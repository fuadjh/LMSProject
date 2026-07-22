using FluentValidation;

namespace Application.ReferenceData.Commands.CreateUniversity;

public sealed class CreateUniversityCommandValidator : AbstractValidator<CreateUniversityCommand>
{
    public CreateUniversityCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
    }
}