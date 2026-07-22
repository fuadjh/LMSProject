using FluentValidation;

namespace Application.Security.Queries.GetRolePermissions;

public sealed class GetRolePermissionsQueryValidator : AbstractValidator<GetRolePermissionsQuery>
{
    public GetRolePermissionsQueryValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
    }
}