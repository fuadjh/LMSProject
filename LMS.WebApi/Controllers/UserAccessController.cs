using Application.Users.Commands.SetUserRoles;
using Application.Users.Commands.SetUserScopes;
using Application.Users.Queries.GetUserAccessDetails;
using Application.Users.Queries.SearchUsers;
using Common.Enums;
using Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Extensions;

namespace WebApi.Controllers;

[ApiController]
[Route("api/user-access")]
public sealed class UserAccessController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserAccessController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("users")]
    [HasPermission(Permissions.Security.UsersRead)]
    public async Task<IActionResult> SearchUsers(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SearchUsersQuery(search),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpGet("users/{userId:guid}")]
    [HasPermission(Permissions.Security.UsersRead)]
    public async Task<IActionResult> GetAccessDetails(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetUserAccessDetailsQuery(userId),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPut("users/{userId:guid}/roles")]
    [HasPermission(Permissions.Security.UserRolesAssign)]
    public async Task<IActionResult> SetRoles(
        Guid userId,
        [FromBody] SetUserRolesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SetUserRolesCommand(
                userId,
                request.Roles),
            cancellationToken);

        return result.Succeeded
            ? NoContent()
            : BadRequest(result.Errors);
    }

    [HttpPut("users/{userId:guid}/scopes")]
    [HasPermission(Permissions.Security.ScopesAssign)]
    public async Task<IActionResult> SetScopes(
        Guid userId,
        [FromBody] SetUserScopesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SetUserScopesCommand(
                userId,
                request.RoleType,
                request.FacultyIds,
                request.MajorIds),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.Errors);
    }
}

public sealed record SetUserRolesRequest(
    IReadOnlyCollection<string> Roles);

public sealed record SetUserScopesRequest(
    UserRoleType RoleType,
    IReadOnlyCollection<Guid> FacultyIds,
    IReadOnlyCollection<Guid> MajorIds);    