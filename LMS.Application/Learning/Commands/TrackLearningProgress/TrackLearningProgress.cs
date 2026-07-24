using Application.Abstractions.Persistence;
using Application.Abstractions.Security;
using Application.Common.Results;
using Common.Enums;
using Domain.Entities.Learning;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Learning.Commands.TrackLearningProgress;

public sealed record OpenLearningItemCommand(
    Guid LearningItemId) : IRequest<Result>;

public sealed record TrackVideoProgressCommand(
    Guid LearningItemId,
    int PositionSeconds,
    int WatchedDeltaSeconds) : IRequest<Result>;

public sealed class OpenLearningItemCommandHandler
    : IRequestHandler<OpenLearningItemCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILearningAccessService _accessService;

    public OpenLearningItemCommandHandler(
        IApplicationDbContext dbContext,
        ILearningAccessService accessService)
    {
        _dbContext = dbContext;
        _accessService = accessService;
    }

    public async Task<Result> Handle(
        OpenLearningItemCommand request,
        CancellationToken cancellationToken)
    {
        var studentProfileId =
            await _accessService.GetCurrentStudentProfileIdAsync(
                cancellationToken);

        if (!studentProfileId.HasValue)
            return Result.Success();

        var data = await (
            from item in _dbContext.LearningItems
            join module in _dbContext.LearningModules
                on item.LearningModuleId equals module.Id
            where item.Id == request.LearningItemId &&
                  item.IsActive &&
                  item.Status ==
                  LearningPublishStatus.Published &&
                  module.IsActive &&
                  module.Status ==
                  LearningPublishStatus.Published
            select new
            {
                Item = item,
                Module = module
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (data is null)
        {
            return Result.NotFound(
                "learning_item.not_found",
                "محتوای آموزشی یافت نشد.");
        }

        if (!await _accessService.CanViewOfferingAsync(
                data.Module.CourseOfferingId,
                cancellationToken))
        {
            return Result.Forbidden(
                "learning.access_denied",
                "دسترسی به این محتوا را ندارید.");
        }

        var now = DateTime.UtcNow;

        if (data.Module.AvailableFromUtc.HasValue &&
            now < data.Module.AvailableFromUtc.Value)
        {
            return Result.Forbidden(
                "learning.not_available_yet",
                "زمان مشاهده این ماژول هنوز نرسیده است.");
        }

        if (data.Module.AvailableUntilUtc.HasValue &&
            now > data.Module.AvailableUntilUtc.Value)
        {
            return Result.Forbidden(
                "learning.availability_ended",
                "زمان مشاهده این ماژول پایان یافته است.");
        }

        var progress =
            await _dbContext.LearningItemProgresses
                .SingleOrDefaultAsync(
                    x =>
                        x.LearningItemId == data.Item.Id &&
                        x.StudentProfileId ==
                        studentProfileId.Value,
                    cancellationToken);

        if (progress is null)
        {
            progress = LearningItemProgress.Create(
                data.Item.Id,
                studentProfileId.Value);

            await _dbContext.AddAsync(
                progress,
                cancellationToken);
        }
        else
        {
            progress.RegisterOpen();
        }

        if (data.Item.Type != LearningItemType.Video)
            progress.Complete();

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}

public sealed class TrackVideoProgressCommandHandler
    : IRequestHandler<TrackVideoProgressCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILearningAccessService _accessService;

    public TrackVideoProgressCommandHandler(
        IApplicationDbContext dbContext,
        ILearningAccessService accessService)
    {
        _dbContext = dbContext;
        _accessService = accessService;
    }

    public async Task<Result> Handle(
        TrackVideoProgressCommand request,
        CancellationToken cancellationToken)
    {
        var studentProfileId =
            await _accessService.GetCurrentStudentProfileIdAsync(
                cancellationToken);

        if (!studentProfileId.HasValue)
        {
            return Result.Forbidden(
                "student.required",
                "فقط دانشجو دارای پیشرفت آموزشی است.");
        }

        var data = await (
            from item in _dbContext.LearningItems
            join module in _dbContext.LearningModules
                on item.LearningModuleId equals module.Id
            where item.Id == request.LearningItemId &&
                  item.Type == LearningItemType.Video &&
                  item.IsActive
            select new
            {
                Item = item,
                Module = module
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (data is null)
        {
            return Result.NotFound(
                "video.not_found",
                "ویدئو یافت نشد.");
        }

        if (!await _accessService.CanViewOfferingAsync(
                data.Module.CourseOfferingId,
                cancellationToken))
        {
            return Result.Forbidden(
                "learning.access_denied",
                "دسترسی به این ویدئو را ندارید.");
        }

        var progress =
            await _dbContext.LearningItemProgresses
                .SingleOrDefaultAsync(
                    x =>
                        x.LearningItemId == data.Item.Id &&
                        x.StudentProfileId ==
                        studentProfileId.Value,
                    cancellationToken);

        if (progress is null)
        {
            progress = LearningItemProgress.Create(
                data.Item.Id,
                studentProfileId.Value);

            await _dbContext.AddAsync(
                progress,
                cancellationToken);
        }

        progress.TrackVideo(
            request.PositionSeconds,
            request.WatchedDeltaSeconds,
            data.Item.DurationSeconds);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}