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

    [HttpGet("{UserId:guid}/access")]
    [HasPermission(Permissions.Security.UsersRead)]
    public async Task<IActionResult> GetAccessDetails(
        Guid UserId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetUserAccessDetailsQuery(UserId),
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

    [HttpDelete("{UserId:guid}")]
    [HasPermission(Permissions.Security.UsersDelete)]
    public async Task<IActionResult> Delete(
        Guid UserId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DeleteUserCommand(UserId),
            cancellationToken);

        return result.ToActionResult(this);
    }
}