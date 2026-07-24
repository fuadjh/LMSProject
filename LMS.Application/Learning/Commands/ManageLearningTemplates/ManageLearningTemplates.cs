using Application.Abstractions.Persistence;
using Application.Abstractions.Security;
using Application.Common.Results;
using Common.Contracts.Learning;
using Domain.Entities.Learning;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Learning.Commands.ManageLearningTemplates;

public sealed record GetLearningTemplatesQuery(
    Guid CourseId)
    : IRequest<Result<IReadOnlyCollection<LearningTemplateListItemDto>>>;

public sealed record CreateTemplateFromOfferingCommand(
    Guid CourseOfferingId,
    string Title,
    bool IsShared)
    : IRequest<Result<Guid>>;

public sealed record ApplyLearningTemplateCommand(
    Guid TemplateId,
    Guid CourseOfferingId)
    : IRequest<Result>;

public sealed class CreateTemplateFromOfferingCommandValidator
    : AbstractValidator<CreateTemplateFromOfferingCommand>
{
    public CreateTemplateFromOfferingCommandValidator()
    {
        RuleFor(x => x.CourseOfferingId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);
    }
}

public sealed class GetLearningTemplatesQueryHandler
    : IRequestHandler<
        GetLearningTemplatesQuery,
        Result<IReadOnlyCollection<LearningTemplateListItemDto>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILearningAccessService _accessService;

    public GetLearningTemplatesQueryHandler(
        IApplicationDbContext dbContext,
        ILearningAccessService accessService)
    {
        _dbContext = dbContext;
        _accessService = accessService;
    }

    public async Task<Result<IReadOnlyCollection<LearningTemplateListItemDto>>> Handle(
        GetLearningTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var instructorProfileId =
            await _accessService.GetCurrentInstructorProfileIdAsync(
                cancellationToken);

        var items = await _dbContext.LearningTemplates
            .AsNoTracking()
            .Where(x =>
                x.CourseId == request.CourseId &&
                x.IsActive &&
                (
                    x.IsShared ||
                    (
                        instructorProfileId.HasValue &&
                        x.InstructorProfileId == instructorProfileId.Value
                    )
                ))
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new LearningTemplateListItemDto(
                x.Id,
                x.CourseId,
                x.Title,
                x.IsShared,
                x.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyCollection<LearningTemplateListItemDto>>
            .Success(items);
    }
}

public sealed class CreateTemplateFromOfferingCommandHandler
    : IRequestHandler<CreateTemplateFromOfferingCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILearningAccessService _accessService;

    public CreateTemplateFromOfferingCommandHandler(
        IApplicationDbContext dbContext,
        ILearningAccessService accessService)
    {
        _dbContext = dbContext;
        _accessService = accessService;
    }

    public async Task<Result<Guid>> Handle(
        CreateTemplateFromOfferingCommand request,
        CancellationToken cancellationToken)
    {
        if (!await _accessService.CanManageOfferingAsync(
                request.CourseOfferingId,
                cancellationToken))
        {
            return Result<Guid>.Forbidden(
                "learning.manage_denied",
                "اجازه مدیریت این ارائه را ندارید.");
        }

        var instructorProfileId =
            await _accessService.GetCurrentInstructorProfileIdAsync(
                cancellationToken);

        if (!instructorProfileId.HasValue)
        {
            return Result<Guid>.Forbidden(
                "instructor.required",
                "ایجاد قالب آموزشی فقط توسط استاد امکان‌پذیر است.");
        }

        var offering = await _dbContext.CourseOfferings
            .SingleOrDefaultAsync(
                x => x.Id == request.CourseOfferingId,
                cancellationToken);

        if (offering is null)
        {
            return Result<Guid>.NotFound(
                "offering.not_found",
                "ارائه درس یافت نشد.");
        }

        var modules = await _dbContext.LearningModules
            .Where(x =>
                x.CourseOfferingId == offering.Id &&
                x.IsActive)
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken);

        if (modules.Count == 0)
        {
            return Result<Guid>.Invalid(
                Error.Validation(
                    "modules",
                    "برای این ارائه محتوایی ثبت نشده است."));
        }

        var moduleIds = modules
            .Select(x => x.Id)
            .ToArray();

        var items = await _dbContext.LearningItems
            .Where(x =>
                moduleIds.Contains(x.LearningModuleId) &&
                x.IsActive)
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken);

        await using var transaction =
            await _dbContext.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var template = LearningTemplate.Create(
                offering.CourseId,
                instructorProfileId.Value,
                request.Title,
                request.IsShared);

            await _dbContext.AddAsync(
                template,
                cancellationToken);

            var moduleMap =
                new Dictionary<Guid, LearningTemplateModule>();

            foreach (var module in modules)
            {
                int? fromOffset = module.AvailableFromUtc.HasValue
                    ? (int)Math.Round(
                        (module.AvailableFromUtc.Value -
                         offering.StartsAtUtc).TotalMinutes)
                    : null;

                int? untilOffset = module.AvailableUntilUtc.HasValue
                    ? (int)Math.Round(
                        (module.AvailableUntilUtc.Value -
                         offering.StartsAtUtc).TotalMinutes)
                    : null;

                var templateModule =
                    LearningTemplateModule.Create(
                        template.Id,
                        module.Title,
                        module.Description,
                        module.Order,
                        null,
                        fromOffset,
                        untilOffset);

                moduleMap[module.Id] = templateModule;

                await _dbContext.AddAsync(
                    templateModule,
                    cancellationToken);
            }

            foreach (var module in modules)
            {
                if (module.PrerequisiteModuleId.HasValue &&
                    moduleMap.TryGetValue(
                        module.PrerequisiteModuleId.Value,
                        out var prerequisite))
                {
                    moduleMap[module.Id]
                        .SetPrerequisite(prerequisite.Id);
                }
            }

            foreach (var item in items)
            {
                if (!moduleMap.TryGetValue(
                        item.LearningModuleId,
                        out var templateModule))
                {
                    return Result<Guid>.Invalid(
                        Error.Validation(
                            "learningItems",
                            "ماژول مربوط به یکی از محتواها یافت نشد."));
                }

                var templateItem =
                    LearningTemplateItem.Create(
                        templateModule.Id,
                        item);

                await _dbContext.AddAsync(
                    templateItem,
                    cancellationToken);
            }

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return Result<Guid>.Success(template.Id);
        }
        catch
        {
            await transaction.RollbackAsync(
                CancellationToken.None);

            throw;
        }
    }
}

public sealed class ApplyLearningTemplateCommandHandler
    : IRequestHandler<ApplyLearningTemplateCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILearningAccessService _accessService;

    public ApplyLearningTemplateCommandHandler(
        IApplicationDbContext dbContext,
        ILearningAccessService accessService)
    {
        _dbContext = dbContext;
        _accessService = accessService;
    }

    public async Task<Result> Handle(
        ApplyLearningTemplateCommand request,
        CancellationToken cancellationToken)
    {
        if (!await _accessService.CanManageOfferingAsync(
                request.CourseOfferingId,
                cancellationToken))
        {
            return Result.Forbidden(
                "learning.manage_denied",
                "اجازه مدیریت ارائه مقصد را ندارید.");
        }

        var offering = await _dbContext.CourseOfferings
            .SingleOrDefaultAsync(
                x => x.Id == request.CourseOfferingId,
                cancellationToken);

        if (offering is null)
        {
            return Result.NotFound(
                "offering.not_found",
                "ارائه مقصد یافت نشد.");
        }

        var template = await _dbContext.LearningTemplates
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x =>
                    x.Id == request.TemplateId &&
                    x.IsActive,
                cancellationToken);

        if (template is null)
        {
            return Result.NotFound(
                "learning_template.not_found",
                "قالب آموزشی یافت نشد.");
        }

        if (template.CourseId != offering.CourseId)
        {
            return Result.Invalid(
                Error.Validation(
                    "templateId",
                    "قالب انتخاب‌شده متعلق به این درس نیست."));
        }

        var hasContent =
            await _dbContext.LearningModules.AnyAsync(
                x =>
                    x.CourseOfferingId == offering.Id &&
                    x.IsActive,
                cancellationToken);

        if (hasContent)
        {
            return Result.Conflict(
                "learning_content.already_exists",
                "ارائه مقصد قبلاً دارای محتوای آموزشی است.");
        }

        var templateModules =
            await _dbContext.LearningTemplateModules
                .AsNoTracking()
                .Where(x =>
                    x.LearningTemplateId == template.Id)
                .OrderBy(x => x.Order)
                .ToListAsync(cancellationToken);

        var templateModuleIds = templateModules
            .Select(x => x.Id)
            .ToArray();

        var templateItems =
            await _dbContext.LearningTemplateItems
                .AsNoTracking()
                .Where(x =>
                    templateModuleIds.Contains(
                        x.LearningTemplateModuleId))
                .OrderBy(x => x.Order)
                .ToListAsync(cancellationToken);

        await using var transaction =
            await _dbContext.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var moduleMap =
                new Dictionary<Guid, LearningModule>();

            foreach (var source in templateModules)
            {
                DateTime? availableFrom =
                    source.AvailableFromOffsetMinutes.HasValue
                        ? offering.StartsAtUtc.AddMinutes(
                            source.AvailableFromOffsetMinutes.Value)
                        : null;

                DateTime? availableUntil =
                    source.AvailableUntilOffsetMinutes.HasValue
                        ? offering.StartsAtUtc.AddMinutes(
                            source.AvailableUntilOffsetMinutes.Value)
                        : null;

                var module = LearningModule.Create(
                    offering.Id,
                    source.Title,
                    source.Description,
                    source.Order,
                    null,
                    availableFrom,
                    availableUntil);

                moduleMap[source.Id] = module;

                await _dbContext.AddAsync(
                    module,
                    cancellationToken);
            }

            foreach (var source in templateModules)
            {
                if (source.PrerequisiteTemplateModuleId.HasValue &&
                    moduleMap.TryGetValue(
                        source.PrerequisiteTemplateModuleId.Value,
                        out var prerequisite))
                {
                    moduleMap[source.Id]
                        .SetPrerequisite(prerequisite.Id);
                }
            }

            foreach (var source in templateItems)
            {
                if (!moduleMap.TryGetValue(
                        source.LearningTemplateModuleId,
                        out var module))
                {
                    return Result.Invalid(
                        Error.Validation(
                            "learningItems",
                            "ماژول مقصد برای یکی از محتواها یافت نشد."));
                }

                var item =
                    source.CreateOfferingItem(module.Id);

                await _dbContext.AddAsync(
                    item,
                    cancellationToken);
            }

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return Result.Success();
        }
        catch
        {
            await transaction.RollbackAsync(
                CancellationToken.None);

            throw;
        }
    }
}