using Application.Abstractions.Persistence;
using Application.Abstractions.Security;
using Application.Common.Results;
using Domain.Entities.Learning;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Learning.Commands.ManageLearningModules;

public sealed record CreateLearningModuleCommand(
    Guid CourseOfferingId,
    string Title,
    string? Description,
    Guid? PrerequisiteModuleId,
    DateTime? AvailableFromUtc,
    DateTime? AvailableUntilUtc)
    : IRequest<Result<Guid>>;

public sealed record UpdateLearningModuleCommand(
    Guid Id,
    string Title,
    string? Description,
    Guid? PrerequisiteModuleId,
    DateTime? AvailableFromUtc,
    DateTime? AvailableUntilUtc)
    : IRequest<Result>;

public sealed record MoveLearningModuleCommand(
    Guid Id,
    int Direction) : IRequest<Result>;

public sealed record PublishLearningModuleCommand(
    Guid Id,
    bool Publish) : IRequest<Result>;

public sealed record DeleteLearningModuleCommand(
    Guid Id) : IRequest<Result>;

public sealed class CreateLearningModuleCommandValidator
    : AbstractValidator<CreateLearningModuleCommand>
{
    public CreateLearningModuleCommandValidator()
    {
        RuleFor(x => x.CourseOfferingId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);

        //RuleFor(x => x.EndDate())
        //    .GreaterThan(x => x.AvailableFromUtc)
        //    .When(x =>
        //        x.AvailableFromUtc.HasValue &&
        //        x.AvailableUntilUtc.HasValue);

        RuleFor(x => x.AvailableUntilUtc)
        .GreaterThan(x => x.AvailableFromUtc)
        .When(x =>
        x.AvailableFromUtc.HasValue &&
        x.AvailableUntilUtc.HasValue);
    }
}

//internal static class LearningModuleCommandExtensions
//{
//    public static DateTime? EndDate(
//        this CreateLearningModuleCommand command) =>
//        command.AvailableUntilUtc;
//}

public sealed class CreateLearningModuleCommandHandler
    : IRequestHandler<CreateLearningModuleCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILearningAccessService _accessService;

    public CreateLearningModuleCommandHandler(
        IApplicationDbContext dbContext,
        ILearningAccessService accessService)
    {
        _dbContext = dbContext;
        _accessService = accessService;
    }

    public async Task<Result<Guid>> Handle(
        CreateLearningModuleCommand request,
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

        if (request.PrerequisiteModuleId.HasValue)
        {
            var prerequisiteExists =
                await _dbContext.LearningModules.AnyAsync(
                    x =>
                        x.Id ==
                        request.PrerequisiteModuleId.Value &&
                        x.CourseOfferingId ==
                        request.CourseOfferingId &&
                        x.IsActive,
                    cancellationToken);

            if (!prerequisiteExists)
            {
                return Result<Guid>.Invalid(
                    Error.Validation(
                        "prerequisiteModuleId",
                        "ماژول پیش‌نیاز معتبر نیست."));
            }
        }

        var maxOrder = await _dbContext.LearningModules
            .Where(x =>
                x.CourseOfferingId ==
                request.CourseOfferingId &&
                x.IsActive)
            .Select(x => (int?)x.Order)
            .MaxAsync(cancellationToken) ?? 0;

        try
        {
            var module = LearningModule.Create(
                request.CourseOfferingId,
                request.Title,
                request.Description,
                maxOrder + 1,
                request.PrerequisiteModuleId,
                request.AvailableFromUtc,
                request.AvailableUntilUtc);

            await _dbContext.AddAsync(
                module,
                cancellationToken);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return Result<Guid>.Success(module.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Invalid(
                Error.Validation(
                    "learning_module.invalid",
                    ex.Message));
        }
    }
}

public sealed class UpdateLearningModuleCommandHandler
    : IRequestHandler<UpdateLearningModuleCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILearningAccessService _accessService;

    public UpdateLearningModuleCommandHandler(
        IApplicationDbContext dbContext,
        ILearningAccessService accessService)
    {
        _dbContext = dbContext;
        _accessService = accessService;
    }

    public async Task<Result> Handle(
        UpdateLearningModuleCommand request,
        CancellationToken cancellationToken)
    {
        var module = await _dbContext.LearningModules
            .SingleOrDefaultAsync(
                x => x.Id == request.Id && x.IsActive,
                cancellationToken);

        if (module is null)
        {
            return Result.NotFound(
                "learning_module.not_found",
                "ماژول آموزشی یافت نشد.");
        }

        if (!await _accessService.CanManageOfferingAsync(
                module.CourseOfferingId,
                cancellationToken))
        {
            return Result.Forbidden(
                "learning.manage_denied",
                "اجازه مدیریت این ارائه را ندارید.");
        }

        if (request.PrerequisiteModuleId == module.Id)
        {
            return Result.Invalid(
                Error.Validation(
                    "prerequisiteModuleId",
                    "ماژول نمی‌تواند پیش‌نیاز خودش باشد."));
        }

        if (request.PrerequisiteModuleId.HasValue)
        {
            var modules = await _dbContext.LearningModules
                .Where(x =>
                    x.CourseOfferingId ==
                    module.CourseOfferingId &&
                    x.IsActive)
                .ToListAsync(cancellationToken);

            var cursor = request.PrerequisiteModuleId;

            while (cursor.HasValue)
            {
                if (cursor.Value == module.Id)
                {
                    return Result.Invalid(
                        Error.Validation(
                            "prerequisiteModuleId",
                            "زنجیره پیش‌نیاز دارای دور است."));
                }

                cursor = modules
                    .FirstOrDefault(x => x.Id == cursor.Value)
                    ?.PrerequisiteModuleId;
            }
        }

        try
        {
            module.Update(
                request.Title,
                request.Description,
                request.PrerequisiteModuleId,
                request.AvailableFromUtc,
                request.AvailableUntilUtc);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.Invalid(
                Error.Validation(
                    "learning_module.invalid",
                    ex.Message));
        }
    }
}

public sealed class MoveLearningModuleCommandHandler
    : IRequestHandler<MoveLearningModuleCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILearningAccessService _accessService;

    public MoveLearningModuleCommandHandler(
        IApplicationDbContext dbContext,
        ILearningAccessService accessService)
    {
        _dbContext = dbContext;
        _accessService = accessService;
    }

    public async Task<Result> Handle(
        MoveLearningModuleCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Direction is not (-1 or 1))
        {
            return Result.Invalid(
                Error.Validation(
                    "direction",
                    "جهت جابه‌جایی معتبر نیست."));
        }

        var module = await _dbContext.LearningModules
            .SingleOrDefaultAsync(
                x => x.Id == request.Id && x.IsActive,
                cancellationToken);

        if (module is null)
        {
            return Result.NotFound(
                "learning_module.not_found",
                "ماژول آموزشی یافت نشد.");
        }

        if (!await _accessService.CanManageOfferingAsync(
                module.CourseOfferingId,
                cancellationToken))
        {
            return Result.Forbidden(
                "learning.manage_denied",
                "اجازه مدیریت این ارائه را ندارید.");
        }

        var modules = await _dbContext.LearningModules
            .Where(x =>
                x.CourseOfferingId ==
                module.CourseOfferingId &&
                x.IsActive)
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken);

        var index = modules.FindIndex(x => x.Id == module.Id);
        var targetIndex = index + request.Direction;

        if (targetIndex < 0 || targetIndex >= modules.Count)
            return Result.Success();

        var target = modules[targetIndex];
        var currentOrder = module.Order;

        module.SetOrder(target.Order);
        target.SetOrder(currentOrder);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}

public sealed class PublishLearningModuleCommandHandler
    : IRequestHandler<PublishLearningModuleCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILearningAccessService _accessService;

    public PublishLearningModuleCommandHandler(
        IApplicationDbContext dbContext,
        ILearningAccessService accessService)
    {
        _dbContext = dbContext;
        _accessService = accessService;
    }

    public async Task<Result> Handle(
        PublishLearningModuleCommand request,
        CancellationToken cancellationToken)
    {
        var module = await _dbContext.LearningModules
            .SingleOrDefaultAsync(
                x => x.Id == request.Id && x.IsActive,
                cancellationToken);

        if (module is null)
        {
            return Result.NotFound(
                "learning_module.not_found",
                "ماژول آموزشی یافت نشد.");
        }

        if (!await _accessService.CanManageOfferingAsync(
                module.CourseOfferingId,
                cancellationToken))
        {
            return Result.Forbidden(
                "learning.manage_denied",
                "اجازه مدیریت این ارائه را ندارید.");
        }

        var items = await _dbContext.LearningItems
            .Where(x =>
                x.LearningModuleId == module.Id &&
                x.IsActive)
            .ToListAsync(cancellationToken);

        if (request.Publish && items.Count == 0)
        {
            return Result.Invalid(
                Error.Validation(
                    "items",
                    "ماژول بدون محتوا قابل انتشار نیست."));
        }

        if (request.Publish)
        {
            module.Publish();

            foreach (var item in items)
                item.Publish();
        }
        else
        {
            module.MoveToDraft();
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}

public sealed class DeleteLearningModuleCommandHandler
    : IRequestHandler<DeleteLearningModuleCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILearningAccessService _accessService;

    public DeleteLearningModuleCommandHandler(
        IApplicationDbContext dbContext,
        ILearningAccessService accessService)
    {
        _dbContext = dbContext;
        _accessService = accessService;
    }

    public async Task<Result> Handle(
        DeleteLearningModuleCommand request,
        CancellationToken cancellationToken)
    {
        var module = await _dbContext.LearningModules
            .SingleOrDefaultAsync(
                x => x.Id == request.Id && x.IsActive,
                cancellationToken);

        if (module is null)
        {
            return Result.NotFound(
                "learning_module.not_found",
                "ماژول آموزشی یافت نشد.");
        }

        if (!await _accessService.CanManageOfferingAsync(
                module.CourseOfferingId,
                cancellationToken))
        {
            return Result.Forbidden(
                "learning.manage_denied",
                "اجازه مدیریت این ارائه را ندارید.");
        }

        var isPrerequisite = await _dbContext.LearningModules
            .AnyAsync(
                x =>
                    x.PrerequisiteModuleId == module.Id &&
                    x.IsActive,
                cancellationToken);

        if (isPrerequisite)
        {
            return Result.Conflict(
                "learning_module.is_prerequisite",
                "این ماژول پیش‌نیاز ماژول دیگری است.");
        }

        module.Deactivate();

        var items = await _dbContext.LearningItems
            .Where(x =>
                x.LearningModuleId == module.Id &&
                x.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var item in items)
            item.Deactivate();

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}