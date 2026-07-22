using FluentValidation;

namespace Application.ReferenceData.Commands.CreateMajor;

public sealed class CreateMajorCommandValidator : AbstractValidator<CreateMajorCommand>
{
    public CreateMajorCommandValidator()
    {
        RuleFor(x => x.FacultyId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
    }
}