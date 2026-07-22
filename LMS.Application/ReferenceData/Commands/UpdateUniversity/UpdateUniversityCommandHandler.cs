using Application.Abstractions.Persistence;
using Application.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ReferenceData.Commands.UpdateUniversity;

public sealed class UpdateUniversityCommandHandler
    : IRequestHandler<UpdateUniversityCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateUniversityCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(
        UpdateUniversityCommand request,
        CancellationToken cancellationToken)
    {
        var university = await _dbContext.Universities
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (university is null)
        {
            return Result.NotFound(
                "university.not_found",
                "دانشگاه یافت نشد.");
        }

        var normalizedCode = request.Code
            .Trim()
            .ToUpperInvariant();

        var duplicateCodeExists = await _dbContext.Universities
            .AnyAsync(
                x => x.Id != request.Id &&
                     x.Code == normalizedCode,
                cancellationToken);

        if (duplicateCodeExists)
        {
            return Result.Conflict(
                "university.code_exists",
                "کد دانشگاه تکراری است.");
        }

        try
        {
            university.Update(
                request.Title,
                request.Code);

            if (request.IsActive)
                university.Activate();
            else
                university.Deactivate();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.Invalid(
                Error.Validation(
                    "university.invalid_data",
                    ex.Message));
        }
    }
}