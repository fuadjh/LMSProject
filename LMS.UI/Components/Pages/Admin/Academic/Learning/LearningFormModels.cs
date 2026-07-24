using Common.Enums;

namespace WebUi.Components.Pages.Admin.Academic.Learning;

public sealed class LearningModuleFormModel
{
    public Guid? Id { get; set; }

    public Guid CourseOfferingId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid? PrerequisiteModuleId { get; set; }

    public DateTime? AvailableFromUtc { get; set; }

    public DateTime? AvailableUntilUtc { get; set; }
}

public sealed class LearningItemFormModel
{
    public Guid? Id { get; set; }

    public Guid CourseOfferingId { get; set; }

    public Guid LearningModuleId { get; set; }

    public string Title { get; set; } = string.Empty;

    public LearningItemType Type { get; set; } =
        LearningItemType.Page;

    public string HtmlContent { get; set; } = string.Empty;

    public string? ExternalUrl { get; set; }

    public string? StorageKey { get; set; }

    public string? HlsMasterPlaylistKey { get; set; }

    public string? OriginalFileName { get; set; }

    public string? ContentType { get; set; }

    public string? CoverStorageKey { get; set; }

    public int? DurationSeconds { get; set; }

    public bool IsRequired { get; set; } = true;
}