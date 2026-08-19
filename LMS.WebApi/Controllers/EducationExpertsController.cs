using Application.Users.Commands.CreateExpert;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/education-experts")]
public sealed class ExpertsController : ControllerBase
{
    private readonly ISender _sender;

    public ExpertsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateExpertCommand command,
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