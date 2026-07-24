using Application.Abstractions.Learning;
using Application.Abstractions.Persistence;
using Application.Abstractions.Security;
using Application.Common.Results;
using Common.Enums;
using Domain.Entities.Learning;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Learning.Commands.ManageLearningItems;

public sealed record SaveLearningItemData(
    string Title,
    LearningItemType Type,
    string? HtmlContent,
    string? ExternalUrl,
    string? StorageKey,
    string? HlsMasterPlaylistKey,
    string? OriginalFileName,
    string? ContentType,
    string? CoverStorageKey,
    int? DurationSeconds,
    bool IsRequired);

public sealed record CreateLearningItemCommand(
    Guid LearningModuleId,
    SaveLearningItemData Data)
    : IRequest<Result<Guid>>;

public sealed record UpdateLearningItemCommand(
    Guid Id,
    SaveLearningItemData Data)
    : IRequest<Result>;

public sealed record MoveLearningItemCommand(
    Guid Id,
    int Direction) : IRequest<Result>;

public sealed record SetLearningItemPublishedCommand(
    Guid Id,
    bool Publish) : IRequest<Result>;

public sealed record DeleteLearningItemCommand(
    Guid Id) : IRequest<Result>;

public sealed class SaveLearningItemDataValidator
    : AbstractValidator<SaveLearningItemData>
{
    public SaveLearningItemDataValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.HtmlContent)
            .NotEmpty()
            .When(x => x.Type == LearningItemType.Page);

        RuleFor(x => x.ExternalUrl)
            .Must(BeValidHttpUrl)
            .When(x =>
                x.Type == LearningItemType.ExternalLink)
            .WithMessage("آدرس لینک معتبر نیست.");

        RuleFor(x => x.StorageKey)
            .NotEmpty()
            .When(x => x.Type == LearningItemType.File);

        RuleFor(x => x.HlsMasterPlaylistKey)
            .NotEmpty()
            .When(x => x.Type == LearningItemType.Video);
    }

    private static bool BeValidHttpUrl(string? value)
    {
        return Uri.TryCreate(
                   value,
                   UriKind.Absolute,
                   out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp ||
                uri.Scheme == Uri.UriSchemeHttps);
    }
}

public sealed class CreateLearningItemCommandValidator
    : AbstractValidator<CreateLearningItemCommand>
{
    public CreateLearningItemCommandValidator()
    {
        RuleFor(x => x.LearningModuleId).NotEmpty();
        RuleFor(x => x.Data)
            .NotNull()
            .SetValidator(new SaveLearningItemDataValidator());
    }
}

public sealed class CreateLearningItemCommandHandler
    : IRequestHandler<CreateLearningItemCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILearningAccessService _accessService;
    private readonly IHtmlContentSanitizer _sanitizer;
    private readonly IPrivateFileStorage _storage;

    public CreateLearningItemCommandHandler(
        IApplicationDbContext dbContext,
        ILearningAccessService accessService,
        IHtmlContentSanitizer sanitizer,
        IPrivateFileStorage storage)
    {
        _dbContext = dbContext;
        _accessService = accessService;
        _sanitizer = sanitizer;
        _storage = storage;
    }

    public async Task<Result<Guid>> Handle(
        CreateLearningItemCommand request,
        CancellationToken cancellationToken)
    {
        var module = await _dbContext.LearningModules
            .SingleOrDefaultAsync(
                x =>
                    x.Id == request.LearningModuleId &&
                    x.IsActive,
                cancellationToken);

        if (module is null)
        {
            return Result<Guid>.NotFound(
                "learning_module.not_found",
                "ماژول آموزشی یافت نشد.");
        }

        if (!await _accessService.CanManageOfferingAsync(
                module.CourseOfferingId,
                cancellationToken))
        {
            return Result<Guid>.Forbidden(
                "learning.manage_denied",
                "اجازه مدیریت این ارائه را ندارید.");
        }

        var assetValidation = await ValidateAssetAsync(
            request.Data,
            cancellationToken);

        if (assetValidation is not null)
            return assetValidation;

        var maxOrder = await _dbContext.LearningItems
            .Where(x =>
                x.LearningModuleId == module.Id &&
                x.IsActive)
            .Select(x => (int?)x.Order)
            .MaxAsync(cancellationToken) ?? 0;

        var html = request.Data.Type ==
                   LearningItemType.Page
            ? _sanitizer.Sanitize(
                request.Data.HtmlContent ?? string.Empty)
            : null;

        try
        {
            var item = LearningItem.Create(
                module.Id,
                request.Data.Title,
                request.Data.Type,
                html,
                request.Data.ExternalUrl,
                request.Data.StorageKey,
                request.Data.HlsMasterPlaylistKey,
                request.Data.OriginalFileName,
                request.Data.ContentType,
                request.Data.CoverStorageKey,
                request.Data.DurationSeconds,
                maxOrder + 1,
                request.Data.IsRequired);

            await _dbContext.AddAsync(
                item,
                cancellationToken);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return Result<Guid>.Success(item.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Invalid(
                Error.Validation(
                    "learning_item.invalid",
                    ex.Message));
        }
    }

    private async Task<Result<Guid>?> ValidateAssetAsync(
        SaveLearningItemData data,
        CancellationToken cancellationToken)
    {
        if (data.Type == LearningItemType.File &&
            !string.IsNullOrWhiteSpace(data.StorageKey) &&
            !await _storage.ExistsAsync(
                data.StorageKey,
                cancellationToken))
        {
            return Result<Guid>.Invalid(
                Error.Validation(
                    "storageKey",
                    "فایل بارگذاری‌شده یافت نشد."));
        }

        if (data.Type == LearningItemType.Video &&
            !string.IsNullOrWhiteSpace(
                data.HlsMasterPlaylistKey) &&
            !await _storage.ExistsAsync(
                data.HlsMasterPlaylistKey,
                cancellationToken))
        {
            return Result<Guid>.Invalid(
                Error.Validation(
                    "hlsMasterPlaylistKey",
                    "فایل HLS یافت نشد."));
        }

        return null;
    }
}

public sealed class UpdateLearningItemCommandHandler
    : IRequestHandler<UpdateLearningItemCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILearningAccessService _accessService;
    private readonly IHtmlContentSanitizer _sanitizer;

    public UpdateLearningItemCommandHandler(
        IApplicationDbContext dbContext,
        ILearningAccessService accessService,
        IHtmlContentSanitizer sanitizer)
    {
        _dbContext = dbContext;
        _accessService = accessService;
        _sanitizer = sanitizer;
    }

    public async Task<Result> Handle(
        UpdateLearningItemCommand request,
        CancellationToken cancellationToken)
    {
        var data = await (
            from item in _dbContext.LearningItems
            join module in _dbContext.LearningModules
                on item.LearningModuleId equals module.Id
            where item.Id == request.Id &&
                  item.IsActive &&
                  module.IsActive
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

        if (!await _accessService.CanManageOfferingAsync(
                data.Module.CourseOfferingId,
                cancellationToken))
        {
            return Result.Forbidden(
                "learning.manage_denied",
                "اجازه مدیریت این ارائه را ندارید.");
        }

        var html = request.Data.Type ==
                   LearningItemType.Page
            ? _sanitizer.Sanitize(
                request.Data.HtmlContent ?? string.Empty)
            : null;

        try
        {
            data.Item.Update(
                request.Data.Title,
                request.Data.Type,
                html,
                request.Data.ExternalUrl,
                request.Data.StorageKey,
                request.Data.HlsMasterPlaylistKey,
                request.Data.OriginalFileName,
                request.Data.ContentType,
                request.Data.CoverStorageKey,
                request.Data.DurationSeconds,
                request.Data.IsRequired);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.Invalid(
                Error.Validation(
                    "learning_item.invalid",
                    ex.Message));
        }
    }
}

public sealed class MoveLearningItemCommandHandler
    : IRequestHandler<MoveLearningItemCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILearningAccessService _accessService;

    public MoveLearningItemCommandHandler(
        IApplicationDbContext dbContext,
        ILearningAccessService accessService)
    {
        _dbContext = dbContext;
        _accessService = accessService;
    }

    public async Task<Result> Handle(
        MoveLearningItemCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Direction is not (-1 or 1))
        {
            return Result.Invalid(
                Error.Validation(
                    "direction",
                    "جهت جابه‌جایی معتبر نیست."));
        }

        var data = await (
            from item in _dbContext.LearningItems
            join module in _dbContext.LearningModules
                on item.LearningModuleId equals module.Id
            where item.Id == request.Id &&
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
                "learning_item.not_found",
                "محتوای آموزشی یافت نشد.");
        }

        if (!await _accessService.CanManageOfferingAsync(
                data.Module.CourseOfferingId,
                cancellationToken))
        {
            return Result.Forbidden(
                "learning.manage_denied",
                "اجازه مدیریت این ارائه را ندارید.");
        }

        var items = await _dbContext.LearningItems
            .Where(x =>
                x.LearningModuleId ==
                data.Item.LearningModuleId &&
                x.IsActive)
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken);

        var index = items.FindIndex(x =>
            x.Id == data.Item.Id);

        var targetIndex = index + request.Direction;

        if (targetIndex < 0 || targetIndex >= items.Count)
            return Result.Success();

        var target = items[targetIndex];
        var currentOrder = data.Item.Order;

        data.Item.SetOrder(target.Order);
        target.SetOrder(currentOrder);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}

public sealed class SetLearningItemPublishedCommandHandler
    : IRequestHandler<SetLearningItemPublishedCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILearningAccessService _accessService;

    public SetLearningItemPublishedCommandHandler(
        IApplicationDbContext dbContext,
        ILearningAccessService accessService)
    {
        _dbContext = dbContext;
        _accessService = accessService;
    }

    public async Task<Result> Handle(
        SetLearningItemPublishedCommand request,
        CancellationToken cancellationToken)
    {
        var data = await (
            from item in _dbContext.LearningItems
            join module in _dbContext.LearningModules
                on item.LearningModuleId equals module.Id
            where item.Id == request.Id &&
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
                "learning_item.not_found",
                "محتوای آموزشی یافت نشد.");
        }

        if (!await _accessService.CanManageOfferingAsync(
                data.Module.CourseOfferingId,
                cancellationToken))
        {
            return Result.Forbidden(
                "learning.manage_denied",
                "اجازه مدیریت این ارائه را ندارید.");
        }

        if (request.Publish)
            data.Item.Publish();
        else
            data.Item.MoveToDraft();

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}

public sealed class DeleteLearningItemCommandHandler
    : IRequestHandler<DeleteLearningItemCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILearningAccessService _accessService;

    public DeleteLearningItemCommandHandler(
        IApplicationDbContext dbContext,
        ILearningAccessService accessService)
    {
        _dbContext = dbContext;
        _accessService = accessService;
    }

    public async Task<Result> Handle(
        DeleteLearningItemCommand request,
        CancellationToken cancellationToken)
    {
        var data = await (
            from item in _dbContext.LearningItems
            join module in _dbContext.LearningModules
                on item.LearningModuleId equals module.Id
            where item.Id == request.Id &&
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
                "learning_item.not_found",
                "محتوای آموزشی یافت نشد.");
        }

        if (!await _accessService.CanManageOfferingAsync(
                data.Module.CourseOfferingId,
                cancellationToken))
        {
            return Result.Forbidden(
                "learning.manage_denied",
                "اجازه مدیریت این ارائه را ندارید.");
        }

        data.Item.Deactivate();

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}