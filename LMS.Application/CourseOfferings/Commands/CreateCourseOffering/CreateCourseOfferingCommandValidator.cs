using FluentValidation;

namespace Application.CourseOfferings.Commands.CreateCourseOffering;

public sealed class CreateCourseOfferingCommandValidator
    : AbstractValidator<CreateCourseOfferingCommand>
{
    public CreateCourseOfferingCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.SemesterId).NotEmpty();

        RuleFor(x => x.SectionCode)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Capacity)
            .InclusiveBetween(1, 500);

        RuleFor(x => x.EndsAtUtc)
            .GreaterThan(x => x.StartsAtUtc)
            .When(x =>
                x.StartsAtUtc.HasValue &&
                x.EndsAtUtc.HasValue);
    }
}