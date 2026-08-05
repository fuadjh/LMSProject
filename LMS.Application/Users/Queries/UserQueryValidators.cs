using Application.Users.Queries.GetUserByNationalCode;
using Application.Users.Queries.GetUserProfileDetails;
using Application.Users.Queries.GetUsers;
using Domain.Entities.Users;
using FluentValidation;

namespace Application.Users.Queries;

public sealed class GetStudentsQueryValidator
    : AbstractValidator<GetStudentsQuery>
{
    public GetStudentsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Search)
            .MaximumLength(200);
    }
}

public sealed class GetInstructorsQueryValidator
    : AbstractValidator<GetInstructorsQuery>
{
    public GetInstructorsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Search)
            .MaximumLength(200);
    }
}

public sealed class GetEducationExpertsQueryValidator
    : AbstractValidator<GetEducationExpertsQuery>
{
    public GetEducationExpertsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Search)
            .MaximumLength(200);
    }
}

public sealed class GetUserProfileDetailsQueryValidator
    : AbstractValidator<GetUserProfileDetailsQuery>
{
    public GetUserProfileDetailsQueryValidator()
    {
        RuleFor(x => x.UserProfileId)
            .NotEmpty();

        RuleFor(x => x.ProfileType)
            .IsInEnum();
    }
}

public sealed class GetUserByNationalCodeQueryValidator
    : AbstractValidator<GetUserByNationalCodeQuery>
{
    public GetUserByNationalCodeQueryValidator()
    {
        RuleFor(x => x.NationalCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("کد ملی الزامی است.")
            .Must(UserProfile.IsValidNationalCode)
            .WithMessage("کد ملی معتبر نیست.");
    }
}