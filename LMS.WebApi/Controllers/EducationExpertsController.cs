using Application.Common.Models;
using Application.Users.Commands.CreateEducationExpert;
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
[Route("api/education-experts")]
public sealed class EducationExpertsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EducationExpertsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.EducationExperts.View)]
    public async Task<IActionResult> GetList(
        [FromQuery] GetEducationExpertsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            query,
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpGet("{userProfileId:guid}")]
    [HasPermission(Permissions.EducationExperts.View)]
    public async Task<IActionResult> GetDetails(
        Guid userProfileId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetUserProfileDetailsQuery(
                userProfileId,
                UserProfileType.EducationExpert),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPost]
    [HasPermission(Permissions.EducationExperts.Create)]
    public async Task<IActionResult> Create(
        CreateEducationExpertRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateEducationExpertCommand(
                request.NationalCode,
                request.UserName,
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.EmployeeCode),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPut("{userProfileId:guid}")]
    [HasPermission(Permissions.EducationExperts.Edit)]
    public async Task<IActionResult> Update(
        Guid userProfileId,
        UpdateEducationExpertRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateEducationExpertCommand(
                userProfileId,
                request.FirstName,
                request.LastName,
                request.Email),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPatch("{userProfileId:guid}/active")]
    [HasPermission(Permissions.EducationExperts.Edit)]
    public async Task<IActionResult> SetActiveStatus(
        Guid userProfileId,
        SetEducationExpertActiveStatusRequest request,
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

public sealed record CreateEducationExpertRequest(
    string NationalCode,
    string UserName,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string EmployeeCode);

public sealed record UpdateEducationExpertRequest(
    string FirstName,
    string LastName,
    string Email);

public sealed record SetEducationExpertActiveStatusRequest(
    bool IsActive);