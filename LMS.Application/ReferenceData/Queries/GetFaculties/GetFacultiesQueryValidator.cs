using Application.Common.Validation;
using FluentValidation;

namespace Application.ReferenceData.Queries.GetFaculties;

public sealed class GetFacultiesQueryValidator
    : PagedQueryValidator<GetFacultiesQuery>
{
    private static readonly string[] AllowedSortColumns =
    [
        "university",
        "title",
        "code",
        "status"
    ];

    public GetFacultiesQueryValidator()
    {
        RuleFor(x => x.UniversityId)
            .NotEqual(Guid.Empty)
            .When(x => x.UniversityId.HasValue)
            .WithMessage("شناسه دانشگاه معتبر نیست.");

        RuleFor(x => x.SortBy)
            .Must(sortBy =>
                string.IsNullOrWhiteSpace(sortBy) ||
                AllowedSortColumns.Contains(
                    sortBy.Trim().ToLowerInvariant()))
            .WithMessage("ستون مرتب‌سازی معتبر نیست.");
    }
}