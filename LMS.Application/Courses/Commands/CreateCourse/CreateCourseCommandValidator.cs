using FluentValidation;

namespace Application.Courses.Commands.CreateCourse;

public sealed class CreateCourseCommandValidator
    : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(x => x.MajorId).NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Units)
            .InclusiveBetween(1, 30);

        RuleFor(x => x.EnrollmentScope)
            .IsInEnum();
    }
}