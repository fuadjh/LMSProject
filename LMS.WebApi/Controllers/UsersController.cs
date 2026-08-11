using Application.Users.Commands.CreateUser;
using Application.Users.Commands.DeleteUser;
using Application.Users.Commands.UpdateUser;
using Application.Users.Queries;
using Common.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public sealed class UsersController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] UserRoleType role,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool descending = false,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetUsersQuery(
                role,
                search,
                page,
                pageSize,
                sortBy,
                descending),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetUser(
        Guid userId,
        [FromQuery] UserRoleType role,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetUserDetailsQuery(userId, role),
            cancellationToken);

        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var result =
            await sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return CreatedAtAction(
            nameof(GetUser),
            new
            {
                userId = result.Value,
                role = command.Role
            },
            new
            {
                id = result.Value
            });
    }

    [HttpPut("{userId:guid}")]
    public async Task<IActionResult> Update(
        Guid userId,
        [FromBody] UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        if (command.UserId != Guid.Empty &&
            command.UserId != userId)
        {
            return BadRequest(new
            {
                errors = new[]
                {
                    "شناسه مسیر با شناسه درخواست یکسان نیست."
                }
            });
        }

        command.UserId = userId;

        var result =
            await sender.Send(command, cancellationToken);

        return result.IsFailure
            ? BadRequest(new { errors = result.Errors })
            : NoContent();
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Delete(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeleteUserCommand(userId),
            cancellationToken);

        return result.IsFailure
            ? BadRequest(new { errors = result.Errors })
            : NoContent();
    }
}