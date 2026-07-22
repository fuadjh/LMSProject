using FluentValidation;

namespace Application.CourseOfferings.Commands.AssignInstructor;

public sealed class AssignInstructorCommandValidator : AbstractValidator<AssignInstructorCommand>
{
    public AssignInstructorCommandValidator()
    {
        RuleFor(x => x.CourseOfferingId).NotEmpty();
        RuleFor(x => x.InstructorProfileId).NotEmpty();
    }
}