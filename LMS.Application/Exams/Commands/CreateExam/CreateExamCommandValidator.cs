using FluentValidation;


namespace Application.Exams.Commands.CreateExam;

public sealed class CreateExamCommandValidator : AbstractValidator<CreateExamCommand>
{
    public CreateExamCommandValidator()
    {
        RuleFor(x => x.CourseOfferingId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DurationMinutes).GreaterThan(0);
        RuleFor(x => x.MaxAttemptsPerStudent).GreaterThan(0);
        RuleFor(x => x.EndsAtUtc).GreaterThan(x => x.StartsAtUtc);
    }
}