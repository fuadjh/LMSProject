using Application.Abstractions.Auth;
using Application.Common.Results;
using MediatR;

namespace Application.Users.Commands.SetUserRoles;

public sealed class SetUserRolesCommandHandler
    : IRequestHandler<SetUserRolesCommand, Result>
{
    private readonly IIdentityService _identityService;

    public SetUserRolesCommandHandler(
        IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(
        SetUserRolesCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _identityService.SetUserRolesAsync(
                request.AuthUserId,
                request.Roles,
                cancellationToken);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Invalid(
                Error.Validation(
                    "roles",
                    ex.Message));
        }
    }
}