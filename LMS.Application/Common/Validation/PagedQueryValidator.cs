using Application.Common.Models;
using FluentValidation;

namespace Application.Common.Validation;

public abstract class PagedQueryValidator<TQuery> : AbstractValidator<TQuery>
    where TQuery : PagedQuery
{
    protected PagedQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("شماره صفحه باید بزرگ‌تر از صفر باشد.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("تعداد رکوردهای صفحه باید بین ۱ تا ۱۰۰ باشد.");

        RuleFor(x => x.Search)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Search));

        RuleFor(x => x.SortBy)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy));
    }
}