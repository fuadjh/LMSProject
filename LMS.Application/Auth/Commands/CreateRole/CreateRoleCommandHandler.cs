using Application.Abstractions.Auth;
using Application.Common.Results;
using MediatR;

namespace Application.Security.Commands.CreateRole;

public sealed class CreateRoleCommandHandler
    : IRequestHandler<CreateRoleCommand, Result<Guid>>
{
    private readonly IRolePermissionService _rolePermissionService;

    public CreateRoleCommandHandler(IRolePermissionService rolePermissionService)
    {
        _rolePermissionService = rolePermissionService;
    }

    public async Task<Result<Guid>> Handle(
        CreateRoleCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var roleId = await _rolePermissionService.CreateRoleAsync(
                request.Name,
                request.Description,
                cancellationToken);

            return Result<Guid>.Success(roleId);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure("role.create_failed", ex.Message);
        }
    }
}