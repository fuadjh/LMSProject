using FluentValidation;

namespace Application.Exams.Commands.SubmitExamAttempt;

public sealed class SubmitExamAttemptCommandValidator : AbstractValidator<SubmitExamAttemptCommand>
{
    public SubmitExamAttemptCommandValidator()
    {
        RuleFor(x => x.SubmissionId).NotEmpty();
        RuleFor(x => x.Answers).NotNull();
    }
}