using FluentValidation;

namespace Application.Exams.Commands.AddQuestionToExam;

public sealed class AddQuestionToExamCommandValidator : AbstractValidator<AddQuestionToExamCommand>
{
    public AddQuestionToExamCommandValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.QuestionId).NotEmpty();
        RuleFor(x => x.Order).GreaterThan(0);
        RuleFor(x => x.Score).GreaterThan(0);
    }
}