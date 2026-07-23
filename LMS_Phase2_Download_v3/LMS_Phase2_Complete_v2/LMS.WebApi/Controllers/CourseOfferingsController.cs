using Application.CourseOfferings.Commands.AssignInstructor;
using Application.CourseOfferings.Commands.CreateCourseOffering;
using Application.CourseOfferings.Commands.DeactivateEnrollment;
using Application.CourseOfferings.Commands.DeleteCourseOffering;
using Application.CourseOfferings.Commands.EnrollStudent;
using Application.CourseOfferings.Commands.UnassignInstructor;
using Application.CourseOfferings.Commands.UpdateCourseOffering;
using Application.CourseOfferings.Queries.GetCourseOfferingById;
using Application.CourseOfferings.Queries.GetCourseOfferings;
using Application.CourseOfferings.Queries.GetOfferingsEnrollments;
using Application.CourseOfferings.Queries.SearchInstructors;
using Application.CourseOfferings.Queries.SearchStudents;
using Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Extensions;

namespace WebApi.Controllers;

[ApiController]
[Route("api/course-offerings")]
public sealed class CourseOfferingsController : ControllerBase
{
    private readonly IMediator _mediator;
    public CourseOfferingsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [HasPermission(Permissions.CourseOfferings.View)]
    public async Task<IActionResult> Lookup(
        [FromQuery] Guid? semesterId, CancellationToken ct) =>
        (await _mediator.Send(new GetCourseOfferingsQuery(semesterId), ct))
        .ToActionResult(this);

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.CourseOfferings.View)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        (await _mediator.Send(new GetCourseOfferingByIdQuery(id), ct))
        .ToActionResult(this);

    [HttpPost]
    [HasPermission(Permissions.CourseOfferings.Create)]
    public async Task<IActionResult> Create(
        CreateOfferingRequest request, CancellationToken ct) =>
        (await _mediator.Send(new CreateCourseOfferingCommand(
            request.CourseId, request.SemesterId,
            request.SectionCode, request.Capacity), ct))
        .ToActionResult(this);

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.CourseOfferings.Create)]
    public async Task<IActionResult> Update(
        Guid id, UpdateOfferingRequest request, CancellationToken ct)
    {
        if (id != request.Id) return BadRequest("شناسه مسیر و بدنه یکسان نیست.");
        return (await _mediator.Send(new UpdateCourseOfferingCommand(
            request.Id, request.SectionCode,
            request.Capacity, request.IsActive), ct)).ToActionResult(this);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.CourseOfferings.Create)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        (await _mediator.Send(new DeleteCourseOfferingCommand(id), ct))
        .ToActionResult(this);

    [HttpPut("{id:guid}/instructor")]
    [HasPermission(Permissions.CourseOfferings.AssignInstructor)]
    public async Task<IActionResult> Assign(
        Guid id, AssignInstructorRequest request, CancellationToken ct) =>
        (await _mediator.Send(
            new AssignInstructorCommand(id, request.InstructorProfileId), ct))
        .ToActionResult(this);

    [HttpDelete("{id:guid}/instructor")]
    [HasPermission(Permissions.CourseOfferings.AssignInstructor)]
    public async Task<IActionResult> Unassign(Guid id, CancellationToken ct) =>
        (await _mediator.Send(new UnassignInstructorCommand(id), ct))
        .ToActionResult(this);

    [HttpGet("{id:guid}/enrollments")]
    [HasPermission(Permissions.CourseOfferings.View)]
    public async Task<IActionResult> Enrollments(Guid id, CancellationToken ct) =>
        (await _mediator.Send(new GetOfferingsEnrollmentsQuery(id), ct))
        .ToActionResult(this);

    [HttpPost("{id:guid}/enrollments")]
    [HasPermission(Permissions.CourseOfferings.EnrollStudent)]
    public async Task<IActionResult> Enroll(
        Guid id, EnrollStudentRequest request, CancellationToken ct) =>
        (await _mediator.Send(
            new EnrollStudentCommand(id, request.StudentProfileId), ct))
        .ToActionResult(this);

    [HttpDelete("enrollments/{id:guid}")]
    [HasPermission(Permissions.CourseOfferings.ManageEnrollment)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct) =>
        (await _mediator.Send(new DeactivateEnrollmentCommand(id), ct))
        .ToActionResult(this);

    [HttpGet("instructors/search")]
    [HasPermission(Permissions.Instructors.View)]
    public async Task<IActionResult> Instructors(
        [FromQuery] string? search, CancellationToken ct) =>
        (await _mediator.Send(new SearchInstructorsQuery(search), ct))
        .ToActionResult(this);

    [HttpGet("students/search")]
    [HasPermission(Permissions.Students.View)]
    public async Task<IActionResult> Students(
        [FromQuery] Guid? majorId,
        [FromQuery] string? search,
        CancellationToken ct) =>
        (await _mediator.Send(new SearchStudentsQuery(majorId, search), ct))
        .ToActionResult(this);
}

public sealed record CreateOfferingRequest(
    Guid CourseId, Guid SemesterId, string SectionCode, int Capacity);

public sealed record UpdateOfferingRequest(
    Guid Id, string SectionCode, int Capacity, bool IsActive);

public sealed record AssignInstructorRequest(Guid InstructorProfileId);
public sealed record EnrollStudentRequest(Guid StudentProfileId);
