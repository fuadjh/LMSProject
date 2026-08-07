using Application.Users.Commands.CreateInstructor;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/instructors")]
public sealed class InstructorsController : ControllerBase
{
    private readonly ISender _sender;

    public InstructorsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateInstructorCommand command,
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

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }
}