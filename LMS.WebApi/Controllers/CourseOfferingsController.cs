using Application.CourseOfferings.Commands.AssignInstructor;
using Application.CourseOfferings.Commands.CreateCourseOffering;
using Application.CourseOfferings.Commands.EnrollStudent;
using Application.CourseOfferings.Queries.GetCourseOfferingById;
using Application.CourseOfferings.Queries.GetCourseOfferings;
using Application.CourseOfferings.Queries.SearchInstructors;
using Application.CourseOfferings.Queries.SearchStudents;
using Application.CourseOfferings.Commands.DeactivateEnrollment;
using Application.CourseOfferings.Commands.UnassignInstructor;
using Application.CourseOfferings.Queries.GetOfferingsEnrollments;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Extensions;
using Common.Security;

namespace WebApi.Controllers;

[ApiController]
[Route("api/course-offerings")]
public sealed class CourseOfferingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CourseOfferingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.CourseOfferings.View)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? semesterId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCourseOfferingsQuery(semesterId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{offeringId:guid}")]
    [HasPermission(Permissions.CourseOfferings.View)]
    public async Task<IActionResult> GetById(Guid offeringId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCourseOfferingByIdQuery(offeringId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [HasPermission(Permissions.CourseOfferings.Create)]
    public async Task<IActionResult> Create(
       CreateCourseOfferingRequest request,
       CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateCourseOfferingCommand(
                request.CourseId,
                request.SemesterId,
                request.SectionCode,
                request.Capacity,
                request.StartsAtUtc,
                request.EndsAtUtc),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPut("{offeringId:guid}/instructor")]
    [HasPermission(Permissions.CourseOfferings.AssignInstructor)]
    public async Task<IActionResult> AssignInstructor(Guid offeringId, AssignInstructorRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new AssignInstructorCommand(offeringId, request.InstructorProfileId),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPost("{offeringId:guid}/enrollments")]
    [HasPermission(Permissions.CourseOfferings.EnrollStudent)]
    public async Task<IActionResult> EnrollStudent(Guid offeringId, EnrollStudentRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new EnrollStudentCommand(offeringId, request.StudentProfileId),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpGet("instructors/search")]
    [HasPermission(Permissions.Instructors.View)]
    public async Task<IActionResult> SearchInstructors([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SearchInstructorsQuery(search), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("students/search")]
    [HasPermission(Permissions.Students.View)]
    public async Task<IActionResult> SearchStudents([FromQuery] Guid? majorId, [FromQuery] string? search, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SearchStudentsQuery(majorId, search), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpDelete("{offeringId:guid}/instructor")]
    [HasPermission(Permissions.CourseOfferings.AssignInstructor)]
    public async Task<IActionResult> UnassignInstructor(Guid offeringId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UnassignInstructorCommand(offeringId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{offeringId:guid}/enrollments")]
    [HasPermission(Permissions.CourseOfferings.View)]
    public async Task<IActionResult> GetEnrollments(Guid offeringId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOfferingsEnrollmentsQuery(offeringId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpDelete("enrollments/{enrollmentId:guid}")]
    [HasPermission(Permissions.CourseOfferings.ManageEnrollment)]
    public async Task<IActionResult> DeactivateEnrollment(Guid enrollmentId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeactivateEnrollmentCommand(enrollmentId), cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record CreateCourseOfferingRequest(
    Guid CourseId,
    Guid SemesterId,
    string SectionCode,
    int Capacity,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc);
public sealed record AssignInstructorRequest(Guid InstructorProfileId);
public sealed record EnrollStudentRequest(Guid StudentProfileId);