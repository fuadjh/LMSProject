using Application.Abstractions.Learning;
using Application.Abstractions.Persistence;
using Application.Abstractions.Security;
using Application.Common.Results;
using Common.Contracts.Learning;
using Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Learning.Commands.ManageLearningAssets;

public sealed record UploadLearningAssetCommand(
    Guid CourseOfferingId,
    LearningItemType Type,
    Stream Content,
    string FileName,
    string ContentType,
    long Size,
    Stream? Cover,
    string? CoverFileName)
    : IRequest<Result<LearningAssetUploadDto>>;

public sealed record GetLearningAssetQuery(
    Guid LearningItemId,
    string? HlsFileName,
    bool Cover)
    : IRequest<Result<LearningAssetStreamDto>>;

public sealed class UploadLearningAssetCommandHandler
    : IRequestHandler<
        UploadLearningAssetCommand,
        Result<LearningAssetUploadDto>>
{
    private static readonly HashSet<string> AllowedFileExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf",
            ".doc",
            ".docx",
            ".xls",
            ".xlsx",
            ".ppt",
            ".pptx",
            ".zip",
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

    private static readonly HashSet<string> AllowedVideoExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4",
            ".mov",
            ".mkv",
            ".webm"
        };

    private readonly ILearningAccessService _accessService;
    private readonly IPrivateFileStorage _storage;
    private readonly IVideoTranscoder _videoTranscoder;

    public UploadLearningAssetCommandHandler(
        ILearningAccessService accessService,
        IPrivateFileStorage storage,
        IVideoTranscoder videoTranscoder)
    {
        _accessService = accessService;
        _storage = storage;
        _videoTranscoder = videoTranscoder;
    }

    public async Task<Result<LearningAssetUploadDto>> Handle(
        UploadLearningAssetCommand request,
        CancellationToken cancellationToken)
    {
        if (!await _accessService.CanManageOfferingAsync(
                request.CourseOfferingId,
                cancellationToken))
        {
            return Result<LearningAssetUploadDto>.Forbidden(
                "learning.manage_denied",
                "اجازه مدیریت این ارائه را ندارید.");
        }

        if (request.Size <= 0)
        {
            return Result<LearningAssetUploadDto>.Invalid(
                Error.Validation(
                    "file",
                    "فایل خالی است."));
        }

        var extension =
            Path.GetExtension(request.FileName)
                .ToLowerInvariant();

        if (request.Type == LearningItemType.File)
        {
            if (!AllowedFileExtensions.Contains(extension))
            {
                return Result<LearningAssetUploadDto>.Invalid(
                    Error.Validation(
                        "file",
                        "نوع فایل مجاز نیست."));
            }

            if (request.Size > 100L * 1024L * 1024L)
            {
                return Result<LearningAssetUploadDto>.Invalid(
                    Error.Validation(
                        "file",
                        "حجم فایل نباید بیشتر از ۱۰۰ مگابایت باشد."));
            }

            var key =
                $"learning/files/{DateTime.UtcNow:yyyy/MM}/" +
                $"{Guid.NewGuid():N}{extension}";

            await _storage.SaveAsync(
                key,
                request.Content,
                cancellationToken);

            return Result<LearningAssetUploadDto>.Success(
                new LearningAssetUploadDto(
                    key,
                    null,
                    null,
                    Path.GetFileName(request.FileName),
                    request.ContentType,
                    request.Size,
                    null));
        }

        if (request.Type == LearningItemType.Video)
        {
            if (!AllowedVideoExtensions.Contains(extension))
            {
                return Result<LearningAssetUploadDto>.Invalid(
                    Error.Validation(
                        "file",
                        "فرمت ویدئو مجاز نیست."));
            }

            if (request.Size > 2L * 1024L * 1024L * 1024L)
            {
                return Result<LearningAssetUploadDto>.Invalid(
                    Error.Validation(
                        "file",
                        "حجم ویدئو بیش از حد مجاز است."));
            }

            var coverExtension =
                string.IsNullOrWhiteSpace(request.CoverFileName)
                    ? null
                    : Path.GetExtension(
                        request.CoverFileName)
                        .ToLowerInvariant();

            var result =
                await _videoTranscoder.TranscodeAsync(
                    request.Content,
                    extension,
                    request.Cover,
                    coverExtension,
                    cancellationToken);

            return Result<LearningAssetUploadDto>.Success(
                new LearningAssetUploadDto(
                    null,
                    result.MasterPlaylistKey,
                    result.CoverStorageKey,
                    Path.GetFileName(request.FileName),
                    request.ContentType,
                    request.Size,
                    result.DurationSeconds));
        }

        return Result<LearningAssetUploadDto>.Invalid(
            Error.Validation(
                "type",
                "آپلود برای این نوع محتوا مجاز نیست."));
    }
}

public sealed class GetLearningAssetQueryHandler
    : IRequestHandler<
        GetLearningAssetQuery,
        Result<LearningAssetStreamDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILearningAccessService _accessService;
    private readonly IPrivateFileStorage _storage;

    public GetLearningAssetQueryHandler(
        IApplicationDbContext dbContext,
        ILearningAccessService accessService,
        IPrivateFileStorage storage)
    {
        _dbContext = dbContext;
        _accessService = accessService;
        _storage = storage;
    }

    public async Task<Result<LearningAssetStreamDto>> Handle(
        GetLearningAssetQuery request,
        CancellationToken cancellationToken)
    {
        var data = await (
            from item in _dbContext.LearningItems
            join module in _dbContext.LearningModules
                on item.LearningModuleId equals module.Id
            where item.Id == request.LearningItemId &&
                  item.IsActive &&
                  module.IsActive
            select new
            {
                Item = item,
                Module = module
            })
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (data is null)
        {
            return Result<LearningAssetStreamDto>.NotFound(
                "learning_asset.not_found",
                "فایل آموزشی یافت نشد.");
        }

        if (!await _accessService.CanViewOfferingAsync(
                data.Module.CourseOfferingId,
                cancellationToken))
        {
            return Result<LearningAssetStreamDto>.Forbidden(
                "learning.access_denied",
                "دسترسی به این فایل را ندارید.");
        }

        string? storageKey;
        string contentType;
        string? downloadName;
        var forceDownload = false;

        if (request.Cover)
        {
            storageKey = data.Item.CoverStorageKey;
            contentType = GetContentType(storageKey);
            downloadName = null;
        }
        else if (data.Item.Type == LearningItemType.File)
        {
            storageKey = data.Item.StorageKey;
            contentType = data.Item.ContentType ??
                          "application/octet-stream";

            downloadName = data.Item.OriginalFileName ??
                           "download";

            forceDownload = true;
        }
        else if (data.Item.Type == LearningItemType.Video)
        {
            storageKey = ResolveVideoStorageKey(
                data.Item.HlsMasterPlaylistKey,
                request.HlsFileName);

            contentType = GetContentType(storageKey);
            downloadName = null;
        }
        else
        {
            return Result<LearningAssetStreamDto>.Invalid(
                Error.Validation(
                    "asset",
                    "این محتوا فایل قابل دریافت ندارد."));
        }

        if (string.IsNullOrWhiteSpace(storageKey))
        {
            return Result<LearningAssetStreamDto>.NotFound(
                "learning_asset.not_found",
                "کلید فایل یافت نشد.");
        }

        var stream = await _storage.OpenReadAsync(
            storageKey,
            cancellationToken);

        if (stream is null)
        {
            return Result<LearningAssetStreamDto>.NotFound(
                "learning_asset.not_found",
                "فایل در فضای ذخیره‌سازی یافت نشد.");
        }

        return Result<LearningAssetStreamDto>.Success(
            new LearningAssetStreamDto(
                stream,
                contentType,
                downloadName,
                forceDownload));
    }

    private static string? ResolveVideoStorageKey(
        string? masterPlaylistKey,
        string? requestedFile)
    {
        if (string.IsNullOrWhiteSpace(masterPlaylistKey))
            return null;

        if (string.IsNullOrWhiteSpace(requestedFile))
            return masterPlaylistKey;

        requestedFile = requestedFile
            .Replace('\\', '/')
            .Trim();

        if (requestedFile.Contains("..") ||
            requestedFile.Contains('/'))
        {
            return null;
        }

        var directory = Path.GetDirectoryName(
                masterPlaylistKey)
            ?.Replace('\\', '/');

        return string.IsNullOrWhiteSpace(directory)
            ? requestedFile
            : $"{directory}/{requestedFile}";
    }

    private static string GetContentType(string? key)
    {
        var extension =
            Path.GetExtension(key ?? string.Empty)
                .ToLowerInvariant();

        return extension switch
        {
            ".m3u8" => "application/vnd.apple.mpegurl",
            ".ts" => "video/mp2t",
            ".m4s" => "video/iso.segment",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }
}