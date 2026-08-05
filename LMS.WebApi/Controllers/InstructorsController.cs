using Application.Common.Models;
using Application.Users.Commands.CreateInstructor;
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
[Route("api/instructors")]
public sealed class InstructorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InstructorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.Instructors.View)]
    public async Task<IActionResult> GetList(
        [FromQuery] GetInstructorsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            query,
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpGet("{userProfileId:guid}")]
    [HasPermission(Permissions.Instructors.View)]
    public async Task<IActionResult> GetDetails(
        Guid userProfileId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetUserProfileDetailsQuery(
                userProfileId,
                UserProfileType.Instructor),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPost]
    [HasPermission(Permissions.Instructors.Create)]
    public async Task<IActionResult> Create(
        CreateInstructorRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateInstructorCommand(
                request.NationalCode,
                request.UserName,
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.PersonnelCode),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPut("{userProfileId:guid}")]
    [HasPermission(Permissions.Instructors.Edit)]
    public async Task<IActionResult> Update(
        Guid userProfileId,
        UpdateInstructorRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateInstructorCommand(
                userProfileId,
                request.FirstName,
                request.LastName,
                request.Email),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPatch("{userProfileId:guid}/active")]
    [HasPermission(Permissions.Instructors.Edit)]
    public async Task<IActionResult> SetActiveStatus(
        Guid userProfileId,
        SetInstructorActiveStatusRequest request,
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

public sealed record CreateInstructorRequest(
    string NationalCode,
    string UserName,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string PersonnelCode);

public sealed record UpdateInstructorRequest(
    string FirstName,
    string LastName,
    string Email);

public sealed record SetInstructorActiveStatusRequest(
    bool IsActive);