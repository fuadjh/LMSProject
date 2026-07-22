
using Application.Users.Commands.CreateEducationExpert;
using Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Extensions;

namespace WebApi.Controllers;

[ApiController]
[Route("api/education-experts")]
public sealed class EducationExpertsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EducationExpertsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [HasPermission(Permissions.EducationExperts.Create)]
    public async Task<IActionResult> Create(CreateEducationExpertRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateEducationExpertCommand(
                request.UserName,
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.EmployeeCode),
            cancellationToken);

        return result.ToActionResult(this);
    }
}

public sealed record CreateEducationExpertRequest(
    string UserName,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string EmployeeCode);