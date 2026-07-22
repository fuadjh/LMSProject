
using Application.Security.Commands.CreateRole;
using Application.Security.Commands.UpdateRolePermissions;
using Application.Security.Queries.GetPermissionCatalog;
using Application.Security.Queries.GetRolePermissions;
using Application.Security.Queries.GetRoles;
using Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Extensions;

namespace WebApi.Controllers;

[ApiController]
[Route("api/roles")]
public sealed class RolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.Security.RolesManage)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetRolesQuery(), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [HasPermission(Permissions.Security.RolesManage)]
    public async Task<IActionResult> Create(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateRoleCommand(request.Name, request.Description), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("permissions/catalog")]
    [HasPermission(Permissions.Security.RolesManage)]
    public async Task<IActionResult> GetPermissionCatalog(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPermissionCatalogQuery(), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{roleId:guid}/permissions")]
    [HasPermission(Permissions.Security.RolesManage)]
    public async Task<IActionResult> GetPermissions(Guid roleId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetRolePermissionsQuery(roleId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut("{roleId:guid}/permissions")]
    [HasPermission(Permissions.Security.RolesManage)]
    public async Task<IActionResult> UpdatePermissions(Guid roleId, UpdateRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateRolePermissionsCommand(roleId, request.Permissions), cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record CreateRoleRequest(string Name, string? Description);
public sealed record UpdateRolePermissionsRequest(IReadOnlyCollection<string> Permissions);