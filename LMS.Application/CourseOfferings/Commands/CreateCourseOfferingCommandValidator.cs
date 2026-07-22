using FluentValidation;

namespace Application.CourseOfferings.Commands.CreateCourseOffering;

public sealed class CreateCourseOfferingCommandValidator : AbstractValidator<CreateCourseOfferingCommand>
{
    public CreateCourseOfferingCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.SemesterId).NotEmpty();
    }
}