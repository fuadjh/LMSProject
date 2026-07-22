
using Application.Semesters.Commands.CreateSemester;
using Application.Semesters.Queries.GetSemesters;
using Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Extensions;

namespace WebApi.Controllers;

[ApiController]
[Route("api/semesters")]
public sealed class SemestersController : ControllerBase
{
    private readonly IMediator _mediator;

    public SemestersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.Semesters.View)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSemestersQuery(), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [HasPermission(Permissions.Semesters.Manage)]
    public async Task<IActionResult> Create(CreateSemesterRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateSemesterCommand(request.Title, request.StartsAtUtc, request.EndsAtUtc),
            cancellationToken);

        return result.ToActionResult(this);
    }
}

public sealed record CreateSemesterRequest(string Title, DateTime StartsAtUtc, DateTime EndsAtUtc);