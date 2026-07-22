using Application.Courses.Commands.CreateCourse;
using Application.Courses.Commands.DeleteCourse;
using Application.Courses.Commands.UpdateCourse;
using Application.Courses.Queries.GetCourses;
using Application.Courses.Queries.GetCoursesPage;
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
    public CoursesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [HasPermission(Permissions.Courses.View)]
    public async Task<IActionResult> Lookup(
        [FromQuery] Guid? majorId, CancellationToken ct) =>
        (await _mediator.Send(new GetCoursesQuery(majorId), ct))
        .ToActionResult(this);

    [HttpGet("paged")]
    [HasPermission(Permissions.Courses.Manage)]
    public async Task<IActionResult> Paged(
        [FromQuery] GetCoursesPageQuery query, CancellationToken ct) =>
        (await _mediator.Send(query, ct)).ToActionResult(this);

    [HttpPost]
    [HasPermission(Permissions.Courses.Manage)]
    public async Task<IActionResult> Create(
        CreateCourseRequest request, CancellationToken ct) =>
        (await _mediator.Send(new CreateCourseCommand(
            request.MajorId, request.Title, request.Code,
            request.Units, request.EnrollmentScope), ct))
        .ToActionResult(this);

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Courses.Manage)]
    public async Task<IActionResult> Update(
        Guid id, UpdateCourseRequest request, CancellationToken ct)
    {
        if (id != request.Id) return BadRequest("شناسه مسیر و بدنه یکسان نیست.");
        return (await _mediator.Send(new UpdateCourseCommand(
            request.Id, request.MajorId, request.Title, request.Code,
            request.Units, request.EnrollmentScope, request.IsActive), ct))
            .ToActionResult(this);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Courses.Manage)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        (await _mediator.Send(new DeleteCourseCommand(id), ct))
        .ToActionResult(this);
}

public sealed record CreateCourseRequest(
    Guid MajorId, string Title, string Code, int Units,
    CourseEnrollmentScope EnrollmentScope);

public sealed record UpdateCourseRequest(
    Guid Id, Guid MajorId, string Title, string Code, int Units,
    CourseEnrollmentScope EnrollmentScope, bool IsActive);
