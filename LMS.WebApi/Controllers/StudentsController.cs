using Application.Common.Models;
using Application.Users.Commands.CreateStudent;
using Application.Users.Commands.SetUserActiveStatus;
using Application.Users.Commands.UpdateUserProfiles;
using Application.Users.Queries.GetUserProfileDetails;
using Application.Users.Queries.GetUsers;
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

    [HttpGet]
    [HasPermission(Permissions.Students.View)]
    public async Task<IActionResult> GetList(
        [FromQuery] GetStudentsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            query,
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpGet("{userProfileId:guid}")]
    [HasPermission(Permissions.Students.View)]
    public async Task<IActionResult> GetDetails(
        Guid userProfileId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetUserProfileDetailsQuery(
                userProfileId,
                UserProfileType.Student),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPost]
    [HasPermission(Permissions.Students.Create)]
    public async Task<IActionResult> Create(
        CreateStudentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateStudentCommand(
                request.NationalCode,
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

    [HttpPut("{userProfileId:guid}")]
    [HasPermission(Permissions.Students.Edit)]
    public async Task<IActionResult> Update(
        Guid userProfileId,
        UpdateStudentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateStudentCommand(
                userProfileId,
                request.FirstName,
                request.LastName,
                request.Email),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPatch("{userProfileId:guid}/active")]
    [HasPermission(Permissions.Students.Edit)]
    public async Task<IActionResult> SetActiveStatus(
        Guid userProfileId,
        SetStudentActiveStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SetUserActiveStatusCommand(
                userProfileId,
                request.IsActive),
            cancellationToken);

        return result.ToActionResult(this);
    }
}

public sealed record CreateStudentRequest(
    string NationalCode,
    string UserName,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string StudentNumber,
    Guid MajorId);

public sealed record UpdateStudentRequest(
    string FirstName,
    string LastName,
    string Email);

public sealed record SetStudentActiveStatusRequest(
    bool IsActive);