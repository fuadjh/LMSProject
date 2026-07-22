using Application.Semesters.Commands.CreateSemester;
using Application.Semesters.Commands.DeleteSemester;
using Application.Semesters.Commands.UpdateSemester;
using Application.Semesters.Queries.GetSemesters;
using Application.Semesters.Queries.GetSemestersPage;
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
    public SemestersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [HasPermission(Permissions.Semesters.View)]
    public async Task<IActionResult> Lookup(CancellationToken ct) =>
        (await _mediator.Send(new GetSemestersQuery(), ct))
        .ToActionResult(this);

    [HttpGet("paged")]
    [HasPermission(Permissions.Semesters.Manage)]
    public async Task<IActionResult> Paged(
        [FromQuery] GetSemestersPageQuery query, CancellationToken ct) =>
        (await _mediator.Send(query, ct)).ToActionResult(this);

    [HttpPost]
    [HasPermission(Permissions.Semesters.Manage)]
    public async Task<IActionResult> Create(
        CreateSemesterRequest request, CancellationToken ct) =>
        (await _mediator.Send(new CreateSemesterCommand(
            request.Title, request.StartsAtUtc, request.EndsAtUtc), ct))
        .ToActionResult(this);

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Semesters.Manage)]
    public async Task<IActionResult> Update(
        Guid id, UpdateSemesterRequest request, CancellationToken ct)
    {
        if (id != request.Id) return BadRequest("شناسه مسیر و بدنه یکسان نیست.");
        return (await _mediator.Send(new UpdateSemesterCommand(
            request.Id, request.Title, request.StartsAtUtc,
            request.EndsAtUtc, request.IsActive), ct)).ToActionResult(this);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Semesters.Manage)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        (await _mediator.Send(new DeleteSemesterCommand(id), ct))
        .ToActionResult(this);
}

public sealed record CreateSemesterRequest(
    string Title, DateTime StartsAtUtc, DateTime EndsAtUtc);

public sealed record UpdateSemesterRequest(
    Guid Id, string Title, DateTime StartsAtUtc,
    DateTime EndsAtUtc, bool IsActive);
