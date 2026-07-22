using Application.Exams.Commands.AddQuestionToExam;
using Application.Exams.Commands.CreateExam;
using Application.Exams.Commands.GradeEssayAnswer;
using Application.Exams.Commands.PublishExam;
using Application.Exams.Commands.StartExamAttempt;
using Application.Exams.Commands.SubmitExamAttempt;
using Application.Exams.Queries.GetExamForStudent;
using Application.Exams.Queries.GetExamQuestions;
using Application.Exams.Queries.GetExamsByOffering;
using Application.Exams.Queries.GetExamSubmissionDetails;
using Application.Exams.Queries.GetExamSubmissions;
using Common.Enums;
using Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Extensions;

namespace WebApi.Controllers;

[ApiController]
[Route("api/exams")]
public sealed class ExamsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExamsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("offering/{courseOfferingId:guid}")]
    [HasPermission(Permissions.Exams.View)]
    public async Task<IActionResult> GetByOffering(Guid courseOfferingId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetExamsByOfferingQuery(courseOfferingId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [HasPermission(Permissions.Exams.Manage)]
    public async Task<IActionResult> Create(CreateExamRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateExamCommand(
                request.CourseOfferingId,
                request.Title,
                request.DeliveryMode,
                request.StartsAtUtc,
                request.EndsAtUtc,
                request.DurationMinutes,
                request.MaxAttemptsPerStudent),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpGet("{examId:guid}/questions")]
    [HasPermission(Permissions.Exams.View)]
    public async Task<IActionResult> GetQuestions(Guid examId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetExamQuestionsQuery(examId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("{examId:guid}/questions")]
    [HasPermission(Permissions.Exams.Manage)]
    public async Task<IActionResult> AddQuestion(Guid examId, AddExamQuestionRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new AddQuestionToExamCommand(examId, request.QuestionId, request.Order, request.Score),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPut("{examId:guid}/publish")]
    [HasPermission(Permissions.Exams.Manage)]
    public async Task<IActionResult> Publish(Guid examId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new PublishExamCommand(examId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{examId:guid}/take")]
    [HasPermission(Permissions.Exams.Take)]
    public async Task<IActionResult> GetForStudent(Guid examId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetExamForStudentQuery(examId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("{examId:guid}/start")]
    [HasPermission(Permissions.Exams.Take)]
    public async Task<IActionResult> Start(Guid examId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new StartExamAttemptCommand(examId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("submissions/{submissionId:guid}/submit")]
    [HasPermission(Permissions.Exams.Take)]
    public async Task<IActionResult> Submit(Guid submissionId, SubmitExamAttemptRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SubmitExamAttemptCommand(
                submissionId,
                request.Answers.Select(x => new SubmitExamAnswerItem(
                    x.QuestionId,
                    x.SelectedOptionId,
                    x.EssayText,
                    x.EssayAttachmentFileName,
                    x.EssayAttachmentPath)).ToArray()),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpGet("{examId:guid}/submissions")]
    [HasPermission(Permissions.Exams.Grade)]
    public async Task<IActionResult> GetSubmissions(Guid examId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetExamSubmissionsQuery(examId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("submissions/{submissionId:guid}")]
    [HasPermission(Permissions.Exams.Grade)]
    public async Task<IActionResult> GetSubmission(Guid submissionId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetExamSubmissionDetailsQuery(submissionId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut("answers/{answerId:guid}/grade")]
    [HasPermission(Permissions.Exams.Grade)]
    public async Task<IActionResult> GradeEssay(Guid answerId, GradeEssayAnswerRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GradeEssayAnswerCommand(answerId, request.AwardedScore), cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record CreateExamRequest(
    Guid CourseOfferingId,
    string Title,
    ExamDeliveryMode DeliveryMode,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    int DurationMinutes,
    int MaxAttemptsPerStudent);

public sealed record AddExamQuestionRequest(
    Guid QuestionId,
    int Order,
    decimal Score);

public sealed record SubmitExamAttemptRequest(
    IReadOnlyCollection<SubmitExamAnswerRequest> Answers);

public sealed record SubmitExamAnswerRequest(
    Guid QuestionId,
    Guid? SelectedOptionId,
    string? EssayText,
    string? EssayAttachmentFileName,
    string? EssayAttachmentPath);

public sealed record GradeEssayAnswerRequest(decimal AwardedScore);