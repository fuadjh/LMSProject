using Application.Users.Commands.CreateStudent;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/students")]
public sealed class StudentsController : ControllerBase
{
    private readonly ISender _sender;

    public StudentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateStudentCommand command,
        CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                errors = result.Errors
            });
        }

        return CreatedAtAction(
            nameof(Create),
            new { id = result.UserId },
            result);
    }
}