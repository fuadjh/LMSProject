using FluentValidation;

namespace Application.Exams.Commands.GradeEssayAnswer;

public sealed class GradeEssayAnswerCommandValidator : AbstractValidator<GradeEssayAnswerCommand>
{
    public GradeEssayAnswerCommandValidator()
    {
        RuleFor(x => x.AnswerId).NotEmpty();
        RuleFor(x => x.AwardedScore).GreaterThanOrEqualTo(0);
    }
}