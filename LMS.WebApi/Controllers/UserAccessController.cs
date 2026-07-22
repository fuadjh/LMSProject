
using Application.Users.Commands.SetUserRoles;
using Application.Users.Commands.SetUserScopes;
using Application.Users.Queries.GetUserAccessDetails;
using Application.Users.Queries.SearchUsers;
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
    public async Task<IActionResult> SearchUsers([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SearchUsersQuery(search), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("profiles/{userProfileId:guid}")]
    [HasPermission(Permissions.Security.UsersRead)]
    public async Task<IActionResult> GetAccessDetails(Guid userProfileId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserAccessDetailsQuery(userProfileId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut("{authUserId:guid}/roles")]
    [HasPermission(Permissions.Security.UserRolesAssign)]
    public async Task<IActionResult> SetRoles(Guid authUserId, SetUserRolesRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SetUserRolesCommand(authUserId, request.Roles), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut("profiles/{userProfileId:guid}/scopes")]
    [HasPermission(Permissions.Security.ScopesAssign)]
    public async Task<IActionResult> SetScopes(Guid userProfileId, SetUserScopesRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SetUserScopesCommand(userProfileId, request.FacultyIds, request.MajorIds), cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record SetUserRolesRequest(IReadOnlyCollection<string> Roles);
public sealed record SetUserScopesRequest(IReadOnlyCollection<Guid> FacultyIds, IReadOnlyCollection<Guid> MajorIds);