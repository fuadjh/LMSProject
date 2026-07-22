using Application.Exams.Commands.CreateQuestion;
using Application.Exams.Queries.GetQuestionBankQuestions;

using Common.Enums;
using Common.Security;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Extensions;

namespace WebApi.Controllers;

[ApiController]
[Route("api/question-bank")]
public sealed class QuestionBankController : ControllerBase
{
    private readonly IMediator _mediator;

    public QuestionBankController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{courseOfferingId:guid}")]
    [HasPermission(Permissions.QuestionBank.View)]
    public async Task<IActionResult> GetQuestions(Guid courseOfferingId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetQuestionBankQuestionsQuery(courseOfferingId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [HasPermission(Permissions.QuestionBank.Manage)]
    public async Task<IActionResult> Create(CreateQuestionRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateQuestionCommand(
                request.CourseOfferingId,
                request.Title,
                request.Body,
                request.Type,
                request.Difficulty,
                request.EvaluationDomain,
                request.SourceType,
                request.SourceDescription,
                request.SuggestedScore,
                request.AttachmentFileName,
                request.AttachmentPath,
                request.Options.Select((x, index) => new CreateQuestionOptionItem(
                    x.Text,
                    x.Order <= 0 ? index + 1 : x.Order,
                    x.IsCorrect)).ToArray()),
            cancellationToken);

        return result.ToActionResult(this);
    }
}

public sealed record CreateQuestionRequest(
    Guid CourseOfferingId,
    string Title,
    string Body,
    QuestionType Type,
    QuestionDifficulty Difficulty,
    EvaluationDomain EvaluationDomain,
    QuestionSourceType SourceType,
    string? SourceDescription,
    decimal SuggestedScore,
    string? AttachmentFileName,
    string? AttachmentPath,
    IReadOnlyCollection<CreateQuestionOptionRequest> Options);

public sealed record CreateQuestionOptionRequest(
    string Text,
    int Order,
    bool IsCorrect);