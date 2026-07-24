using Application.Abstractions.Persistence;
using Application.Abstractions.Security;
using Application.Common.Results;
using Common.Contracts.Learning;
using Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Learning.Queries.GetOfferingLearningContent;

public sealed class GetOfferingLearningContentQueryHandler
    : IRequestHandler<
        GetOfferingLearningContentQuery,
        Result<LearningContentDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILearningAccessService _accessService;

    public GetOfferingLearningContentQueryHandler(
        IApplicationDbContext dbContext,
        ILearningAccessService accessService)
    {
        _dbContext = dbContext;
        _accessService = accessService;
    }

    public async Task<Result<LearningContentDto>> Handle(
        GetOfferingLearningContentQuery request,
        CancellationToken cancellationToken)
    {
        var canView =
            await _accessService.CanViewOfferingAsync(
                request.CourseOfferingId,
                cancellationToken);

        if (!canView)
        {
            return Result<LearningContentDto>.Forbidden(
                "learning.access_denied",
                "دسترسی به محتوای این ارائه را ندارید.");
        }

        var canManage =
            await _accessService.CanManageOfferingAsync(
                request.CourseOfferingId,
                cancellationToken);

        var header = await (
            from offering in _dbContext.CourseOfferings
            join course in _dbContext.Courses
                on offering.CourseId equals course.Id
            join semester in _dbContext.Semesters
                on offering.SemesterId equals semester.Id
            where offering.Id == request.CourseOfferingId
            select new
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                SemesterTitle = semester.Title,
                offering.SectionCode
            })
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (header is null)
        {
            return Result<LearningContentDto>.NotFound(
                "offering.not_found",
                "ارائه درس یافت نشد.");
        }

        var moduleQuery = _dbContext.LearningModules
            .AsNoTracking()
            .Where(x =>
                x.CourseOfferingId ==
                request.CourseOfferingId &&
                x.IsActive);

        if (!canManage)
        {
            moduleQuery = moduleQuery.Where(
                x => x.Status ==
                     LearningPublishStatus.Published);
        }

        var modules = await moduleQuery
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken);

        var moduleIds = modules
            .Select(x => x.Id)
            .ToArray();

        var itemQuery = _dbContext.LearningItems
            .AsNoTracking()
            .Where(x =>
                moduleIds.Contains(x.LearningModuleId) &&
                x.IsActive);

        if (!canManage)
        {
            itemQuery = itemQuery.Where(
                x => x.Status ==
                     LearningPublishStatus.Published);
        }

        var items = await itemQuery
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken);

        var studentProfileId =
            await _accessService
                .GetCurrentStudentProfileIdAsync(
                    cancellationToken);

        var progress = studentProfileId.HasValue
            ? await _dbContext.LearningItemProgresses
                .AsNoTracking()
                .Where(x =>
                    x.StudentProfileId ==
                    studentProfileId.Value &&
                    items.Select(i => i.Id)
                        .Contains(x.LearningItemId))
                .ToListAsync(cancellationToken)
            : [];

        var completedItemIds = progress
            .Where(x => x.CompletedAtUtc.HasValue)
            .Select(x => x.LearningItemId)
            .ToHashSet();

        var requiredItemsByModule = items
            .Where(x => x.IsRequired)
            .GroupBy(x => x.LearningModuleId)
            .ToDictionary(
                x => x.Key,
                x => x.Select(i => i.Id).ToArray());

        var now = DateTime.UtcNow;

        var moduleDtos = modules.Select(module =>
        {
            var isLocked = false;

            if (!canManage && studentProfileId.HasValue)
            {
                if (module.AvailableFromUtc.HasValue &&
                    now < module.AvailableFromUtc.Value)
                {
                    isLocked = true;
                }

                if (module.AvailableUntilUtc.HasValue &&
                    now > module.AvailableUntilUtc.Value)
                {
                    isLocked = true;
                }

                if (module.PrerequisiteModuleId.HasValue)
                {
                    requiredItemsByModule.TryGetValue(
                        module.PrerequisiteModuleId.Value,
                        out var requiredIds);

                    requiredIds ??= [];

                    if (requiredIds.Any(id =>
                            !completedItemIds.Contains(id)))
                    {
                        isLocked = true;
                    }
                }
            }

            var prerequisiteTitle = modules
                .FirstOrDefault(x =>
                    x.Id == module.PrerequisiteModuleId)
                ?.Title;

            var moduleItems = items
                .Where(x =>
                    x.LearningModuleId == module.Id)
                .Select(item =>
                {
                    var itemProgress = progress
                        .FirstOrDefault(x =>
                            x.LearningItemId == item.Id);

                    return new LearningItemDto(
     item.Id,
     item.LearningModuleId,
     item.Title,
     item.Type,
     item.Order,
     item.IsRequired,
     item.Status,
     item.HtmlContent,
     item.ExternalUrl,
     canManage ? item.StorageKey : null,
     canManage ? item.HlsMasterPlaylistKey : null,
     item.OriginalFileName,
     item.ContentType,
     canManage ? item.CoverStorageKey : null,
     item.DurationSeconds,
     itemProgress?.CompletedAtUtc.HasValue == true,
     itemProgress?.ViewCount ?? 0,
     itemProgress?.LastPositionSeconds ?? 0);
                })
                .ToArray();

            return new LearningModuleDto(
                module.Id,
                module.Title,
                module.Description,
                module.Order,
                module.PrerequisiteModuleId,
                prerequisiteTitle,
                module.AvailableFromUtc,
                module.AvailableUntilUtc,
                module.Status,
                isLocked,
                moduleItems);
        }).ToArray();

        return Result<LearningContentDto>.Success(
     new LearningContentDto(
         request.CourseOfferingId,
         header.CourseId,
         header.CourseTitle,
         header.SemesterTitle,
         header.SectionCode,
         canManage,
         moduleDtos));
    }
}