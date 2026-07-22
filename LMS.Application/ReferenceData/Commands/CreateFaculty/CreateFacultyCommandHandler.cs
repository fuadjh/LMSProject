using Application.Abstractions.Persistence;
using Application.Common.Results;
using Domain.Entities.Academics;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ReferenceData.Commands.CreateFaculty;

public sealed class CreateFacultyCommandHandler
    : IRequestHandler<CreateFacultyCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateFacultyCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(
        CreateFacultyCommand request,
        CancellationToken cancellationToken)
    {
        var universityExists = await _dbContext.Universities
            .AnyAsync(
                x => x.Id == request.UniversityId && x.IsActive,
                cancellationToken);

        if (!universityExists)
        {
            return Result<Guid>.NotFound(
                "faculty.university_not_found",
                "دانشگاه انتخاب‌شده یافت نشد یا غیرفعال است.");
        }

        var normalizedCode = request.Code
            .Trim()
            .ToUpperInvariant();

        var duplicateCodeExists = await _dbContext.Faculties
            .AnyAsync(
                x => x.UniversityId == request.UniversityId &&
                     x.Code == normalizedCode,
                cancellationToken);

        if (duplicateCodeExists)
        {
            return Result<Guid>.Conflict(
                "faculty.code_exists",
                "کد دانشکده در دانشگاه انتخاب‌شده تکراری است.");
        }

        try
        {
            var faculty = Faculty.Create(
                request.UniversityId,
                request.Title.Trim(),
                normalizedCode);

            await _dbContext.AddAsync(
                faculty,
                cancellationToken);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return Result<Guid>.Success(faculty.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Invalid(
                Error.Validation(
                    "faculty.invalid_data",
                    ex.Message));
        }
    }
}