using FluentValidation;

namespace Application.Users.Queries.GetUserAccessDetails;

public sealed class GetUserAccessDetailsQueryValidator : AbstractValidator<GetUserAccessDetailsQuery>
{
    public GetUserAccessDetailsQueryValidator()
    {
        RuleFor(x => x.UserProfileId).NotEmpty();
    }
}