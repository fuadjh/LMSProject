using Application.Abstractions.Identity;
using Application.Common.Models;
using MediatR;

namespace Application.Users.Commands.SetUserRoles;

public sealed class SetUserRolesCommandHandler
    : IRequestHandler<
        SetUserRolesCommand,
        IdentityOperationResult>
{
    private readonly IUserAccountService _userAccountService;

    public SetUserRolesCommandHandler(
        IUserAccountService userAccountService)
    {
        _userAccountService = userAccountService;
    }

    public Task<IdentityOperationResult> Handle(
        SetUserRolesCommand request,
        CancellationToken cancellationToken)
    {
        return _userAccountService.SetRolesAsync(
            request.UserId,
            request.Roles,
            cancellationToken);
    }
}