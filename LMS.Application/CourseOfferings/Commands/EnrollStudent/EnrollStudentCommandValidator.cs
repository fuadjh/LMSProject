using FluentValidation;

namespace Application.CourseOfferings.Commands.EnrollStudent;

public sealed class EnrollStudentCommandValidator : AbstractValidator<EnrollStudentCommand>
{
    public EnrollStudentCommandValidator()
    {
        RuleFor(x => x.CourseOfferingId).NotEmpty();
        RuleFor(x => x.StudentProfileId).NotEmpty();
    }
}