
using Application.Users.Commands.CreateStudent;
using Common.Security;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Extensions;

namespace WebApi.Controllers;

[ApiController]
[Route("api/students")]
public sealed class StudentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StudentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [HasPermission(Permissions.Students.Create)]
    public async Task<IActionResult> Create(CreateStudentRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateStudentCommand(
                request.UserName,
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.StudentNumber,
                request.MajorId),
            cancellationToken);

        return result.ToActionResult(this);
    }
}

public sealed record CreateStudentRequest(
    string UserName,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string StudentNumber,
    Guid MajorId);