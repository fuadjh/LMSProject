using Application.Courses.Commands.CreateCourse;
using Application.Courses.Commands.DeleteCourse;
using Application.Courses.Commands.UpdateCourse;
using Application.Courses.Queries.GetCourses;
using Common.Enums;
using Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Extensions;

namespace WebApi.Controllers;

[ApiController]
[Route("api/courses")]
public sealed class CoursesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CoursesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.Courses.View)]
    public async Task<IActionResult> GetAll(
     [FromQuery] GetCoursesQuery query,
     CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            query,
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPost]
    [HasPermission(Permissions.Courses.Manage)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCourseRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateCourseCommand(
                request.MajorId,
                request.Title,
                request.Code,
                request.Units,
                request.EnrollmentScope),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Courses.Manage)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCourseRequest request,
        CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            return BadRequest(
                "شناسه مسیر و بدنه درخواست یکسان نیست.");
        }

        var result = await _mediator.Send(
            new UpdateCourseCommand(
                request.Id,
                request.MajorId,
                request.Title,
                request.Code,
                request.Units,
                request.EnrollmentScope,
                request.IsActive),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Courses.Manage)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DeleteCourseCommand(id),
            cancellationToken);

        return result.ToActionResult(this);
    }
}

public sealed record CreateCourseRequest(
    Guid MajorId,
    string Title,
    string Code,
    int Units,
    CourseEnrollmentScope EnrollmentScope);

public sealed record UpdateCourseRequest(
    Guid Id,
    Guid MajorId,
    string Title,
    string Code,
    int Units,
    CourseEnrollmentScope EnrollmentScope,
    bool IsActive);