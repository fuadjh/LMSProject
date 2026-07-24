using Application.Courses.Commands.CreateCourse;
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
    public async Task<IActionResult> GetAll([FromQuery] Guid? majorId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCoursesQuery(majorId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [HasPermission(Permissions.Courses.Manage)]
    public async Task<IActionResult> Create(
      CreateCourseRequest request,
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
}

public sealed record CreateCourseRequest(
    Guid MajorId,
    string Title,
    string Code,
    int Units,
    CourseEnrollmentScope EnrollmentScope);