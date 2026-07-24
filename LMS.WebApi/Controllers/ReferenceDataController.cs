using Application.ReferenceData.Commands.CreateFaculty;
using Application.ReferenceData.Commands.CreateMajor;
using Application.ReferenceData.Commands.CreateUniversity;
using Application.ReferenceData.Commands.DeleteFaculty;
using Application.ReferenceData.Commands.DeleteMajor;
using Application.ReferenceData.Commands.DeleteUniversity;
using Application.ReferenceData.Commands.UpdateFaculty;
using Application.ReferenceData.Commands.UpdateMajor;
using Application.ReferenceData.Commands.UpdateUniversity;
using Application.ReferenceData.Queries.GetFaculties;
using Application.ReferenceData.Queries.GetMajors;
using Application.ReferenceData.Queries.GetUniversities;
using Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Extensions;

namespace WebApi.Controllers;

[ApiController]
[Route("api/reference-data")]
public sealed class ReferenceDataController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReferenceDataController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("faculties")]
    //[HasPermission(Permissions.ReferenceData.View)]
    public async Task<IActionResult> GetFaculties(
      [FromQuery] GetFacultiesQuery query,
      CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            query,
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPost("faculties")]
    //[HasPermission(Permissions.ReferenceData.Manage)]
    public async Task<IActionResult> CreateFaculty(
      [FromBody] CreateFacultyCommand command,
      CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            command,
            cancellationToken);

        return result.ToActionResult(this);
    }
    [HttpPut("faculties/{id:guid}")]
    //[HasPermission(Permissions.ReferenceData.Manage)]
    public async Task<IActionResult> UpdateFaculty(
    Guid id,
    [FromBody] UpdateFacultyCommand command,
    CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(
                "شناسه مسیر و شناسه بدنه درخواست یکسان نیستند.");
        }

        var result = await _mediator.Send(
            command,
            cancellationToken);

        return result.ToActionResult(this);
    }
    [HttpDelete("faculties/{id:guid}")]
    //[HasPermission(Permissions.ReferenceData.Manage)]
    public async Task<IActionResult> DeleteFaculty(
    Guid id,
    CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DeleteFacultyCommand(id),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpGet("majors")]
    //[HasPermission(Permissions.ReferenceData.View)]
    public async Task<IActionResult> GetMajors(
      [FromQuery] GetMajorsQuery query,
      CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            query,
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPost("majors")]
    //[HasPermission(Permissions.ReferenceData.Manage)]
    public async Task<IActionResult> CreateMajor(
      [FromBody] CreateMajorCommand command,
      CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            command,
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPut("majors/{id:guid}")]
    //[HasPermission(Permissions.ReferenceData.Manage)]
    public async Task<IActionResult> UpdateMajor(
    Guid id,
    [FromBody] UpdateMajorCommand command,
    CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(
                "شناسه مسیر و شناسه بدنه درخواست یکسان نیستند.");
        }

        var result = await _mediator.Send(
            command,
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpDelete("majors/{id:guid}")]
    //[HasPermission(Permissions.ReferenceData.Manage)]
    public async Task<IActionResult> DeleteMajor(
    Guid id,
    CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DeleteMajorCommand(id),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpGet("universities")]
    //[HasPermission(Permissions.ReferenceData.View)]
    public async Task<IActionResult> GetUniversities(
     [FromQuery] GetUniversitiesQuery query,
     CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPost("universities")]
    //[HasPermission(Permissions.ReferenceData.Manage)]
    public async Task<IActionResult> CreateUniversity(CreateUniversityRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateUniversityCommand(request.Title, request.Code), cancellationToken);
        return result.ToActionResult(this);
    }
    [HttpPut("universities/{id:guid}")]
    //[HasPermission(Permissions.ReferenceData.Manage)]
    public async Task<IActionResult> UpdateUniversity(
    Guid id,
    [FromBody] UpdateUniversityCommand command,
    CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("شناسه مسیر و بدنه درخواست یکسان نیست.");

        var result = await _mediator.Send(
            command,
            cancellationToken);

        return result.ToActionResult(this);
    }
    [HttpDelete("universities/{id:guid}")]
    //[HasPermission(Permissions.ReferenceData.Manage)]
    public async Task<IActionResult> DeleteUniversity(
    Guid id,
    CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DeleteUniversityCommand(id),
            cancellationToken);

        return result.ToActionResult(this);
    }
}

public sealed record CreateUniversityRequest(string Title, string Code);