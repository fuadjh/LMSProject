using Common.Enums;

using FluentValidation;

namespace Application.Exams.Commands.CreateQuestion;

public sealed class CreateQuestionCommandValidator : AbstractValidator<CreateQuestionCommand>
{
    public CreateQuestionCommandValidator()
    {
        RuleFor(x => x.CourseOfferingId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Body).NotEmpty();
        RuleFor(x => x.SuggestedScore).GreaterThan(0);

        RuleFor(x => x.Options)
            .Must((cmd, options) =>
            {
                if (cmd.Type == QuestionType.Essay)
                    return options is null || options.Count == 0;

                if (options is null || options.Count < 2)
                    return false;

                return options.Count(x => x.IsCorrect) == 1;
            })
            .WithMessage("برای سوال چندگزینه‌ای باید حداقل دو گزینه و دقیقاً یک گزینه صحیح وجود داشته باشد.");
    }
}