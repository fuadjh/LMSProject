
using Application.Users.Commands.CreateInstructor;
using Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Extensions;

namespace WebApi.Controllers;

[ApiController]
[Route("api/instructors")]
public sealed class InstructorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InstructorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [HasPermission(Permissions.Instructors.Create)]
    public async Task<IActionResult> Create(CreateInstructorRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateInstructorCommand(
                request.UserName,
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.PersonnelCode),
            cancellationToken);

        return result.ToActionResult(this);
    }
}

public sealed record CreateInstructorRequest(
    string UserName,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string PersonnelCode);