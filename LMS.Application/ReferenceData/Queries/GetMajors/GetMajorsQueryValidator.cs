using Application.Common.Validation;
using FluentValidation;

namespace Application.ReferenceData.Queries.GetMajors;

public sealed class GetMajorsQueryValidator
    : PagedQueryValidator<GetMajorsQuery>
{
    private static readonly string[] AllowedSortColumns =
    [
        "university",
        "faculty",
        "title",
        "code",
        "status"
    ];

    public GetMajorsQueryValidator()
    {
        RuleFor(x => x.FacultyId)
            .NotEqual(Guid.Empty)
            .When(x => x.FacultyId.HasValue)
            .WithMessage("شناسه دانشکده معتبر نیست.");

        RuleFor(x => x.SortBy)
            .Must(sortBy =>
                string.IsNullOrWhiteSpace(sortBy) ||
                AllowedSortColumns.Contains(
                    sortBy.Trim().ToLowerInvariant()))
            .WithMessage("ستون مرتب‌سازی معتبر نیست.");
    }
}