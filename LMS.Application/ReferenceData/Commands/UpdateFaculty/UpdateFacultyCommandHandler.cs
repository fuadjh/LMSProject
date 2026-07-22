using Application.Abstractions.Persistence;
using Application.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ReferenceData.Commands.UpdateFaculty;

public sealed class UpdateFacultyCommandHandler
    : IRequestHandler<UpdateFacultyCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateFacultyCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(
        UpdateFacultyCommand request,
        CancellationToken cancellationToken)
    {
        var faculty = await _dbContext.Faculties
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (faculty is null)
        {
            return Result.NotFound(
                "faculty.not_found",
                "دانشکده یافت نشد.");
        }

        var universityExists = await _dbContext.Universities
            .AnyAsync(
                x => x.Id == request.UniversityId && x.IsActive,
                cancellationToken);

        if (!universityExists)
        {
            return Result.NotFound(
                "faculty.university_not_found",
                "دانشگاه انتخاب‌شده یافت نشد یا غیرفعال است.");
        }

        var normalizedCode = request.Code
            .Trim()
            .ToUpperInvariant();

        var duplicateCodeExists = await _dbContext.Faculties
            .AnyAsync(
                x => x.Id != request.Id &&
                     x.UniversityId == request.UniversityId &&
                     x.Code == normalizedCode,
                cancellationToken);

        if (duplicateCodeExists)
        {
            return Result.Conflict(
                "faculty.code_exists",
                "کد دانشکده در دانشگاه انتخاب‌شده تکراری است.");
        }

        try
        {
            faculty.Update(
                request.UniversityId,
                request.Title.Trim(),
                normalizedCode);

            if (request.IsActive)
                faculty.Activate();
            else
                faculty.Deactivate();

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.Invalid(
                Error.Validation(
                    "faculty.invalid_data",
                    ex.Message));
        }
    }
}