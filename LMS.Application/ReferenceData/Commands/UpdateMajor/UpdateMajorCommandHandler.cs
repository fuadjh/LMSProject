using Application.Abstractions.Persistence;
using Application.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ReferenceData.Commands.UpdateMajor;

public sealed class UpdateMajorCommandHandler
    : IRequestHandler<UpdateMajorCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateMajorCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(
        UpdateMajorCommand request,
        CancellationToken cancellationToken)
    {
        var major = await _dbContext.Majors
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (major is null)
        {
            return Result.NotFound(
                "major.not_found",
                "رشته یافت نشد.");
        }

        var facultyExists = await _dbContext.Faculties
            .AnyAsync(
                x => x.Id == request.FacultyId && x.IsActive,
                cancellationToken);

        if (!facultyExists)
        {
            return Result.NotFound(
                "major.faculty_not_found",
                "دانشکده انتخاب‌شده یافت نشد یا غیرفعال است.");
        }

        var normalizedCode = request.Code
            .Trim()
            .ToUpperInvariant();

        var duplicateCodeExists = await _dbContext.Majors
            .AnyAsync(
                x => x.Id != request.Id &&
                     x.FacultyId == request.FacultyId &&
                     x.Code == normalizedCode,
                cancellationToken);

        if (duplicateCodeExists)
        {
            return Result.Conflict(
                "major.code_exists",
                "کد رشته در دانشکده انتخاب‌شده تکراری است.");
        }

        try
        {
            if (major.FacultyId != request.FacultyId)
                major.MoveToFaculty(request.FacultyId);

            major.Update(
                request.Title.Trim(),
                normalizedCode);

            if (request.IsActive)
                major.Activate();
            else
                major.Deactivate();

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.Invalid(
                Error.Validation(
                    "major.invalid_data",
                    ex.Message));
        }
    }
}
