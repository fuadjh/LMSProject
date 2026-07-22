using FluentValidation;

namespace Application.Courses.Commands.UpdateCourse;

public sealed class UpdateCourseCommandValidator
    : AbstractValidator<UpdateCourseCommand>
{
    public UpdateCourseCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
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