using FluentValidation;

namespace Application.Semesters.Commands.CreateSemester;

public sealed class CreateSemesterCommandValidator : AbstractValidator<CreateSemesterCommand>
{
    public CreateSemesterCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StartsAtUtc).NotEmpty();
        RuleFor(x => x.EndsAtUtc).NotEmpty().GreaterThan(x => x.StartsAtUtc);
    }
}