using Application.Abstractions.Auth;
using Application.Common.Results;
using MediatR;

namespace Application.Security.Commands.UpdateRolePermissions;

public sealed class UpdateRolePermissionsCommandHandler : IRequestHandler<UpdateRolePermissionsCommand, Result>
{
    private readonly IRolePermissionService _service;

    public UpdateRolePermissionsCommandHandler(IRolePermissionService service)
    {
        _service = service;
    }

    public async Task<Result> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _service.UpdateRolePermissionsAsync(request.RoleId, request.Permissions, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure("role_permissions.update_failed", ex.Message);
        }
    }
}