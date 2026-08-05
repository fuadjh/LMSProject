using Application.Security.Queries.GetRoles;
using Application.Users.Commands.DeleteUser;
using Application.Users.Queries.GetUserByNationalCode;
using Application.Users.Queries.GetUserAccessDetails;
using Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Extensions;

namespace WebApi.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("by-national-code/{nationalCode}")]
    [HasPermission(Permissions.Security.UsersRead)]
    public async Task<IActionResult> GetByNationalCode(
        string nationalCode,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetUserByNationalCodeQuery(nationalCode),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpGet("{userProfileId:guid}/access")]
    [HasPermission(Permissions.Security.UsersRead)]
    public async Task<IActionResult> GetAccessDetails(
        Guid userProfileId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetUserAccessDetailsQuery(userProfileId),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpGet("roles")]
    [HasPermission(Permissions.Security.UserRolesAssign)]
    public async Task<IActionResult> GetRoles(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetRolesQuery(),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpDelete("{userProfileId:guid}")]
    [HasPermission(Permissions.Security.UsersDelete)]
    public async Task<IActionResult> Delete(
        Guid userProfileId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DeleteUserCommand(userProfileId),
            cancellationToken);

        return result.ToActionResult(this);
    }
}