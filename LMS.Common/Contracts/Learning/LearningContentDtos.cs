using Common.Enums;

namespace Common.Contracts.Learning;

public sealed record LearningContentDto(
    Guid CourseOfferingId,
    Guid CourseId,
    string CourseTitle,
    string SemesterTitle,
    string SectionCode,
    bool CanManage,
    IReadOnlyCollection<LearningModuleDto> Modules);

public sealed record LearningModuleDto(
    Guid Id,
    string Title,
    string? Description,
    int Order,
    Guid? PrerequisiteModuleId,
    string? PrerequisiteModuleTitle,
    DateTime? AvailableFromUtc,
    DateTime? AvailableUntilUtc,
    LearningPublishStatus Status,
    bool IsLocked,
    IReadOnlyCollection<LearningItemDto> Items);

public sealed record LearningItemDto(
    Guid Id,
    Guid LearningModuleId,
    string Title,
    LearningItemType Type,
    int Order,
    bool IsRequired,
    LearningPublishStatus Status,
    string? HtmlContent,
    string? ExternalUrl,
    string? StorageKey,
    string? HlsMasterPlaylistKey,
    string? OriginalFileName,
    string? ContentType,
    string? CoverStorageKey,
    int? DurationSeconds,
    bool IsCompleted,
    int ViewCount,
    int LastPositionSeconds);

public sealed record LearningTemplateListItemDto(
    Guid Id,
    Guid CourseId,
    string Title,
    bool IsShared,
    DateTime CreatedAtUtc);

public sealed record LearningAssetUploadDto(
    string? StorageKey,
    string? HlsMasterPlaylistKey,
    string? CoverStorageKey,
    string OriginalFileName,
    string ContentType,
    long Size,
    int? DurationSeconds);

public sealed record LearningAssetStreamDto(
    Stream Stream,
    string ContentType,
    string? DownloadFileName,
    bool ForceDownload);