using Common.Enums;
using Domain.Common;

namespace Domain.Entities.Learning;

public sealed class LearningModule : BaseEntity
{
    private LearningModule()
    {
    }

    public Guid CourseOfferingId { get; private set; }

    public string Title { get; private set; } = default!;

    public string? Description { get; private set; }

    public int Order { get; private set; }

    public Guid? PrerequisiteModuleId { get; private set; }

    public DateTime? AvailableFromUtc { get; private set; }

    public DateTime? AvailableUntilUtc { get; private set; }

    public LearningPublishStatus Status { get; private set; }

    public bool IsActive { get; private set; }

    public static LearningModule Create(
        Guid courseOfferingId,
        string title,
        string? description,
        int order,
        Guid? prerequisiteModuleId,
        DateTime? availableFromUtc,
        DateTime? availableUntilUtc)
    {
        Validate(
            courseOfferingId,
            title,
            order,
            availableFromUtc,
            availableUntilUtc);

        return new LearningModule
        {
            Id = Guid.NewGuid(),
            CourseOfferingId = courseOfferingId,
            Title = title.Trim(),
            Description = description?.Trim(),
            Order = order,
            PrerequisiteModuleId = prerequisiteModuleId,
            AvailableFromUtc = availableFromUtc,
            AvailableUntilUtc = availableUntilUtc,
            Status = LearningPublishStatus.Draft,
            IsActive = true
        };
    }

    public void Update(
        string title,
        string? description,
        Guid? prerequisiteModuleId,
        DateTime? availableFromUtc,
        DateTime? availableUntilUtc)
    {
        Validate(
            CourseOfferingId,
            title,
            Order,
            availableFromUtc,
            availableUntilUtc);

        Title = title.Trim();
        Description = description?.Trim();
        PrerequisiteModuleId = prerequisiteModuleId;
        AvailableFromUtc = availableFromUtc;
        AvailableUntilUtc = availableUntilUtc;
    }

    public void SetPrerequisite(Guid? prerequisiteModuleId)
    {
        if (prerequisiteModuleId == Id)
            throw new ArgumentException(
                "A module cannot be its own prerequisite.");

        PrerequisiteModuleId = prerequisiteModuleId;
    }

    public void SetOrder(int order)
    {
        if (order <= 0)
            throw new ArgumentException(
                "Order must be greater than zero.");

        Order = order;
    }

    public void Publish() =>
        Status = LearningPublishStatus.Published;

    public void MoveToDraft() =>
        Status = LearningPublishStatus.Draft;

    public void Deactivate() => IsActive = false;

    private static void Validate(
        Guid courseOfferingId,
        string title,
        int order,
        DateTime? availableFromUtc,
        DateTime? availableUntilUtc)
    {
        if (courseOfferingId == Guid.Empty)
            throw new ArgumentException(
                "CourseOfferingId is required.");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.");

        if (title.Trim().Length > 200)
            throw new ArgumentException(
                "Title cannot exceed 200 characters.");

        if (order <= 0)
            throw new ArgumentException(
                "Order must be greater than zero.");

        if (availableFromUtc.HasValue &&
            availableUntilUtc.HasValue &&
            availableUntilUtc <= availableFromUtc)
        {
            throw new ArgumentException(
                "AvailableUntilUtc must be after AvailableFromUtc.");
        }
    }
}

public sealed class LearningItem : BaseEntity
{
    private LearningItem()
    {
    }

    public Guid LearningModuleId { get; private set; }

    public string Title { get; private set; } = default!;

    public LearningItemType Type { get; private set; }

    public string? HtmlContent { get; private set; }

    public string? ExternalUrl { get; private set; }

    public string? StorageKey { get; private set; }

    public string? HlsMasterPlaylistKey { get; private set; }

    public string? OriginalFileName { get; private set; }

    public string? ContentType { get; private set; }

    public string? CoverStorageKey { get; private set; }

    public int? DurationSeconds { get; private set; }

    public int Order { get; private set; }

    public bool IsRequired { get; private set; }

    public LearningPublishStatus Status { get; private set; }

    public bool IsActive { get; private set; }

    public static LearningItem Create(
        Guid learningModuleId,
        string title,
        LearningItemType type,
        string? htmlContent,
        string? externalUrl,
        string? storageKey,
        string? hlsMasterPlaylistKey,
        string? originalFileName,
        string? contentType,
        string? coverStorageKey,
        int? durationSeconds,
        int order,
        bool isRequired)
    {
        Validate(
            learningModuleId,
            title,
            type,
            htmlContent,
            externalUrl,
            storageKey,
            hlsMasterPlaylistKey,
            order);

        return new LearningItem
        {
            Id = Guid.NewGuid(),
            LearningModuleId = learningModuleId,
            Title = title.Trim(),
            Type = type,
            HtmlContent = htmlContent,
            ExternalUrl = externalUrl?.Trim(),
            StorageKey = storageKey,
            HlsMasterPlaylistKey = hlsMasterPlaylistKey,
            OriginalFileName = originalFileName?.Trim(),
            ContentType = contentType?.Trim(),
            CoverStorageKey = coverStorageKey,
            DurationSeconds = durationSeconds,
            Order = order,
            IsRequired = isRequired,
            Status = LearningPublishStatus.Draft,
            IsActive = true
        };
    }

    public void Update(
        string title,
        LearningItemType type,
        string? htmlContent,
        string? externalUrl,
        string? storageKey,
        string? hlsMasterPlaylistKey,
        string? originalFileName,
        string? contentType,
        string? coverStorageKey,
        int? durationSeconds,
        bool isRequired)
    {
        Validate(
            LearningModuleId,
            title,
            type,
            htmlContent,
            externalUrl,
            storageKey,
            hlsMasterPlaylistKey,
            Order);

        Title = title.Trim();
        Type = type;
        HtmlContent = htmlContent;
        ExternalUrl = externalUrl?.Trim();
        StorageKey = storageKey;
        HlsMasterPlaylistKey = hlsMasterPlaylistKey;
        OriginalFileName = originalFileName?.Trim();
        ContentType = contentType?.Trim();
        CoverStorageKey = coverStorageKey;
        DurationSeconds = durationSeconds;
        IsRequired = isRequired;
    }

    public void SetOrder(int order)
    {
        if (order <= 0)
            throw new ArgumentException(
                "Order must be greater than zero.");

        Order = order;
    }

    public void Publish() =>
        Status = LearningPublishStatus.Published;

    public void MoveToDraft() =>
        Status = LearningPublishStatus.Draft;

    public void Deactivate() => IsActive = false;

    private static void Validate(
        Guid moduleId,
        string title,
        LearningItemType type,
        string? htmlContent,
        string? externalUrl,
        string? storageKey,
        string? hlsMasterPlaylistKey,
        int order)
    {
        if (moduleId == Guid.Empty)
            throw new ArgumentException(
                "LearningModuleId is required.");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.");

        if (title.Trim().Length > 200)
            throw new ArgumentException(
                "Title cannot exceed 200 characters.");

        if (order <= 0)
            throw new ArgumentException(
                "Order must be greater than zero.");

        if (type == LearningItemType.Page &&
            string.IsNullOrWhiteSpace(htmlContent))
        {
            throw new ArgumentException(
                "Page content is required.");
        }

        if (type == LearningItemType.ExternalLink &&
            string.IsNullOrWhiteSpace(externalUrl))
        {
            throw new ArgumentException(
                "External URL is required.");
        }

        if (type == LearningItemType.File &&
            string.IsNullOrWhiteSpace(storageKey))
        {
            throw new ArgumentException(
                "File storage key is required.");
        }

        if (type == LearningItemType.Video &&
            string.IsNullOrWhiteSpace(hlsMasterPlaylistKey))
        {
            throw new ArgumentException(
                "HLS master playlist is required.");
        }
    }
}

public sealed class LearningItemProgress : BaseEntity
{
    private LearningItemProgress()
    {
    }

    public Guid LearningItemId { get; private set; }

    public Guid StudentProfileId { get; private set; }

    public DateTime FirstOpenedAtUtc { get; private set; }

    public DateTime LastOpenedAtUtc { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public int ViewCount { get; private set; }

    public int LastPositionSeconds { get; private set; }

    public int WatchedSeconds { get; private set; }

    public static LearningItemProgress Create(
        Guid learningItemId,
        Guid studentProfileId)
    {
        if (learningItemId == Guid.Empty)
            throw new ArgumentException(
                "LearningItemId is required.");

        if (studentProfileId == Guid.Empty)
            throw new ArgumentException(
                "StudentProfileId is required.");

        var now = DateTime.UtcNow;

        return new LearningItemProgress
        {
            Id = Guid.NewGuid(),
            LearningItemId = learningItemId,
            StudentProfileId = studentProfileId,
            FirstOpenedAtUtc = now,
            LastOpenedAtUtc = now,
            ViewCount = 1
        };
    }

    public void RegisterOpen()
    {
        ViewCount++;
        LastOpenedAtUtc = DateTime.UtcNow;
    }

    public void Complete()
    {
        CompletedAtUtc ??= DateTime.UtcNow;
    }

    public void TrackVideo(
        int positionSeconds,
        int watchedDeltaSeconds,
        int? durationSeconds)
    {
        if (positionSeconds < 0)
            positionSeconds = 0;

        watchedDeltaSeconds =
            Math.Clamp(watchedDeltaSeconds, 0, 30);

        LastPositionSeconds = positionSeconds;
        WatchedSeconds += watchedDeltaSeconds;
        LastOpenedAtUtc = DateTime.UtcNow;

        if (durationSeconds is > 0)
        {
            var completionThreshold =
                (int)Math.Ceiling(durationSeconds.Value * 0.90m);

            if (WatchedSeconds >= completionThreshold)
                Complete();
        }
    }
}

public sealed class LearningTemplate : BaseEntity
{
    private LearningTemplate()
    {
    }

    public Guid CourseId { get; private set; }

    public Guid InstructorProfileId { get; private set; }

    public string Title { get; private set; } = default!;

    public bool IsShared { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public bool IsActive { get; private set; }

    public static LearningTemplate Create(
        Guid courseId,
        Guid instructorProfileId,
        string title,
        bool isShared)
    {
        if (courseId == Guid.Empty)
            throw new ArgumentException("CourseId is required.");

        if (instructorProfileId == Guid.Empty)
            throw new ArgumentException(
                "InstructorProfileId is required.");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.");

        return new LearningTemplate
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            InstructorProfileId = instructorProfileId,
            Title = title.Trim(),
            IsShared = isShared,
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };
    }
}

public sealed class LearningTemplateModule : BaseEntity
{
    private LearningTemplateModule()
    {
    }

    public Guid LearningTemplateId { get; private set; }

    public string Title { get; private set; } = default!;

    public string? Description { get; private set; }

    public int Order { get; private set; }

    public Guid? PrerequisiteTemplateModuleId { get; private set; }

    public int? AvailableFromOffsetMinutes { get; private set; }

    public int? AvailableUntilOffsetMinutes { get; private set; }

    public static LearningTemplateModule Create(
        Guid learningTemplateId,
        string title,
        string? description,
        int order,
        Guid? prerequisiteTemplateModuleId,
        int? availableFromOffsetMinutes,
        int? availableUntilOffsetMinutes)
    {
        if (learningTemplateId == Guid.Empty)
            throw new ArgumentException(
                "LearningTemplateId is required.");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.");

        if (order <= 0)
            throw new ArgumentException(
                "Order must be greater than zero.");

        return new LearningTemplateModule
        {
            Id = Guid.NewGuid(),
            LearningTemplateId = learningTemplateId,
            Title = title.Trim(),
            Description = description?.Trim(),
            Order = order,
            PrerequisiteTemplateModuleId =
                prerequisiteTemplateModuleId,
            AvailableFromOffsetMinutes =
                availableFromOffsetMinutes,
            AvailableUntilOffsetMinutes =
                availableUntilOffsetMinutes
        };
    }

    public void SetPrerequisite(Guid? prerequisiteId) =>
        PrerequisiteTemplateModuleId = prerequisiteId;
}

public sealed class LearningTemplateItem : BaseEntity
{
    private LearningTemplateItem()
    {
    }

    public Guid LearningTemplateModuleId { get; private set; }

    public string Title { get; private set; } = default!;

    public LearningItemType Type { get; private set; }

    public string? HtmlContent { get; private set; }

    public string? ExternalUrl { get; private set; }

    public string? StorageKey { get; private set; }

    public string? HlsMasterPlaylistKey { get; private set; }

    public string? OriginalFileName { get; private set; }

    public string? ContentType { get; private set; }

    public string? CoverStorageKey { get; private set; }

    public int? DurationSeconds { get; private set; }

    public int Order { get; private set; }

    public bool IsRequired { get; private set; }

    public static LearningTemplateItem Create(
        Guid templateModuleId,
        LearningItem source)
    {
        return new LearningTemplateItem
        {
            Id = Guid.NewGuid(),
            LearningTemplateModuleId = templateModuleId,
            Title = source.Title,
            Type = source.Type,
            HtmlContent = source.HtmlContent,
            ExternalUrl = source.ExternalUrl,
            StorageKey = source.StorageKey,
            HlsMasterPlaylistKey =
                source.HlsMasterPlaylistKey,
            OriginalFileName = source.OriginalFileName,
            ContentType = source.ContentType,
            CoverStorageKey = source.CoverStorageKey,
            DurationSeconds = source.DurationSeconds,
            Order = source.Order,
            IsRequired = source.IsRequired
        };
    }

    public LearningItem CreateOfferingItem(Guid moduleId)
    {
        return LearningItem.Create(
            moduleId,
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
            Order,
            IsRequired);
    }
}