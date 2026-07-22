using Common.Contracts.Auth;
using LMS.Application.Auth.Commands.Login;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<SignInDataDto>> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("نام کاربری و کلمه عبور الزامی است.");

        var result = await _mediator.Send(
            new LoginCommand(request.UserName.Trim(), request.Password),
            cancellationToken);

        if (!result.IsSuccess || result.Value is null)
            return Unauthorized("نام کاربری یا کلمه عبور نادرست است.");

        return Ok(result.Value);
    }




}