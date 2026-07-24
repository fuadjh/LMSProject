using Application.Learning.Commands.ManageLearningItems;
using Application.Learning.Commands.ManageLearningModules;
using Application.Learning.Commands.ManageLearningTemplates;
using Application.Learning.Commands.TrackLearningProgress;
using Application.Learning.Queries.GetOfferingLearningContent;
using Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Extensions;
using Application.Learning.Commands.ManageLearningAssets;
using Common.Enums;

namespace WebApi.Controllers;

[ApiController]
[Route("api/learning-content")]
public sealed class LearningContentController : ControllerBase
{
    private readonly IMediator _mediator;

    public LearningContentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("offerings/{offeringId:guid}")]
    [HasPermission(Permissions.Learning.View)]
    public async Task<IActionResult> GetOfferingContent(
        Guid offeringId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetOfferingLearningContentQuery(offeringId),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPost("modules")]
    [HasPermission(Permissions.Learning.Manage)]
    public async Task<IActionResult> CreateModule(
        SaveLearningModuleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateLearningModuleCommand(
                request.CourseOfferingId,
                request.Title,
                request.Description,
                request.PrerequisiteModuleId,
                request.AvailableFromUtc,
                request.AvailableUntilUtc),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPut("modules/{id:guid}")]
    [HasPermission(Permissions.Learning.Manage)]
    public async Task<IActionResult> UpdateModule(
        Guid id,
        SaveLearningModuleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateLearningModuleCommand(
                id,
                request.Title,
                request.Description,
                request.PrerequisiteModuleId,
                request.AvailableFromUtc,
                request.AvailableUntilUtc),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPut("modules/{id:guid}/move")]
    [HasPermission(Permissions.Learning.Manage)]
    public async Task<IActionResult> MoveModule(
        Guid id,
        MoveRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new MoveLearningModuleCommand(
                id,
                request.Direction),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPut("modules/{id:guid}/publish")]
    [HasPermission(Permissions.Learning.Manage)]
    public async Task<IActionResult> PublishModule(
        Guid id,
        PublishRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new PublishLearningModuleCommand(
                id,
                request.Publish),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpDelete("modules/{id:guid}")]
    [HasPermission(Permissions.Learning.Manage)]
    public async Task<IActionResult> DeleteModule(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DeleteLearningModuleCommand(id),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPost("items")]
    [HasPermission(Permissions.Learning.Manage)]
    public async Task<IActionResult> CreateItem(
        SaveLearningItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateLearningItemCommand(
                request.LearningModuleId,
                request.ToData()),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPut("items/{id:guid}")]
    [HasPermission(Permissions.Learning.Manage)]
    public async Task<IActionResult> UpdateItem(
        Guid id,
        SaveLearningItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateLearningItemCommand(
                id,
                request.ToData()),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPut("items/{id:guid}/move")]
    [HasPermission(Permissions.Learning.Manage)]
    public async Task<IActionResult> MoveItem(
        Guid id,
        MoveRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new MoveLearningItemCommand(
                id,
                request.Direction),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPut("items/{id:guid}/publish")]
    [HasPermission(Permissions.Learning.Manage)]
    public async Task<IActionResult> PublishItem(
        Guid id,
        PublishRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SetLearningItemPublishedCommand(
                id,
                request.Publish),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpDelete("items/{id:guid}")]
    [HasPermission(Permissions.Learning.Manage)]
    public async Task<IActionResult> DeleteItem(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DeleteLearningItemCommand(id),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPost("items/{id:guid}/open")]
    [HasPermission(Permissions.Learning.View)]
    public async Task<IActionResult> OpenItem(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new OpenLearningItemCommand(id),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPost("items/{id:guid}/video-progress")]
    [HasPermission(Permissions.Learning.View)]
    public async Task<IActionResult> TrackVideo(
        Guid id,
        TrackVideoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new TrackVideoProgressCommand(
                id,
                request.PositionSeconds,
                request.WatchedDeltaSeconds),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpGet("templates")]
    [HasPermission(Permissions.Learning.Manage)]
    public async Task<IActionResult> GetTemplates(
        [FromQuery] Guid courseId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetLearningTemplatesQuery(courseId),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPost("templates/from-offering")]
    [HasPermission(Permissions.Learning.Manage)]
    public async Task<IActionResult> CreateTemplate(
        CreateTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateTemplateFromOfferingCommand(
                request.CourseOfferingId,
                request.Title,
                request.IsShared),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPost("templates/{templateId:guid}/apply")]
    [HasPermission(Permissions.Learning.Manage)]
    public async Task<IActionResult> ApplyTemplate(
        Guid templateId,
        ApplyTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ApplyLearningTemplateCommand(
                templateId,
                request.CourseOfferingId),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPost("assets")]
    [HasPermission(Permissions.Learning.Manage)]
    [RequestSizeLimit(2L * 1024L * 1024L * 1024L)]
    public async Task<IActionResult> UploadAsset(
    [FromQuery] Guid courseOfferingId,
    [FromQuery] LearningItemType type,
    IFormFile file,
    IFormFile? cover,
    CancellationToken cancellationToken)
    {
        if (file is null || file.Length <= 0)
            return BadRequest("فایل خالی است.");

        await using var fileStream =
            file.OpenReadStream();

        await using var coverStream =
            cover?.OpenReadStream();

        var result = await _mediator.Send(
            new UploadLearningAssetCommand(
                courseOfferingId,
                type,
                fileStream,
                file.FileName,
                file.ContentType,
                file.Length,
                coverStream,
                cover?.FileName),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpGet("items/{itemId:guid}/file")]
    [HasPermission(Permissions.Learning.View)]
    public async Task<IActionResult> DownloadFile(
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetLearningAssetQuery(
                itemId,
                null,
                false),
            cancellationToken);

        if (!result.IsSuccess || result.Value is null)
            return result.ToActionResult(this);

        return File(
            result.Value.Stream,
            result.Value.ContentType,
            result.Value.DownloadFileName,
            enableRangeProcessing: true);
    }

    [HttpGet("items/{itemId:guid}/cover")]
    [HasPermission(Permissions.Learning.View)]
    public async Task<IActionResult> GetCover(
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetLearningAssetQuery(
                itemId,
                null,
                true),
            cancellationToken);

        if (!result.IsSuccess || result.Value is null)
            return result.ToActionResult(this);

        return File(
            result.Value.Stream,
            result.Value.ContentType,
            enableRangeProcessing: true);
    }

    [HttpGet("items/{itemId:guid}/hls")]
    [HasPermission(Permissions.Learning.View)]
    public async Task<IActionResult> GetMasterPlaylist(
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetLearningAssetQuery(
                itemId,
                null,
                false),
            cancellationToken);

        if (!result.IsSuccess || result.Value is null)
            return result.ToActionResult(this);

        return File(
            result.Value.Stream,
            result.Value.ContentType,
            enableRangeProcessing: true);
    }

    [HttpGet("items/{itemId:guid}/hls/{fileName}")]
    [HasPermission(Permissions.Learning.View)]
    public async Task<IActionResult> GetHlsFile(
        Guid itemId,
        string fileName,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetLearningAssetQuery(
                itemId,
                fileName,
                false),
            cancellationToken);

        if (!result.IsSuccess || result.Value is null)
            return result.ToActionResult(this);

        return File(
            result.Value.Stream,
            result.Value.ContentType,
            enableRangeProcessing: true);
    }
}

public sealed record SaveLearningModuleRequest(
    Guid CourseOfferingId,
    string Title,
    string? Description,
    Guid? PrerequisiteModuleId,
    DateTime? AvailableFromUtc,
    DateTime? AvailableUntilUtc);

public sealed record SaveLearningItemRequest(
    Guid LearningModuleId,
    string Title,
    Common.Enums.LearningItemType Type,
    string? HtmlContent,
    string? ExternalUrl,
    string? StorageKey,
    string? HlsMasterPlaylistKey,
    string? OriginalFileName,
    string? ContentType,
    string? CoverStorageKey,
    int? DurationSeconds,
    bool IsRequired)
{
    public SaveLearningItemData ToData() =>
        new(
            Title,
            Type,
            HtmlContent,
            ExternalUrl,
            StorageKey,
            HlsMasterPlaylistKey,
            OriginalFileName,
            ContentType,
            CoverStorageKey,
            DurationSeconds,
            IsRequired);
}

public sealed record MoveRequest(int Direction);

public sealed record PublishRequest(bool Publish);

public sealed record TrackVideoRequest(
    int PositionSeconds,
    int WatchedDeltaSeconds);

public sealed record CreateTemplateRequest(
    Guid CourseOfferingId,
    string Title,
    bool IsShared);

public sealed record ApplyTemplateRequest(
    Guid CourseOfferingId);