using Application.Abstractions.Persistence;
using Application.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ReferenceData.Commands.DeleteMajor;

public sealed class DeleteMajorCommandHandler
    : IRequestHandler<DeleteMajorCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteMajorCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(
        DeleteMajorCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var affectedRows = await _dbContext.Majors
                .Where(x => x.Id == request.Id)
                .ExecuteDeleteAsync(cancellationToken);

            if (affectedRows == 0)
            {
                return Result.NotFound(
                    "major.not_found",
                    "رشته یافت نشد.");
            }

            return Result.Success();
        }
        catch (DbUpdateException)
        {
            return Result.Conflict(
                "major.has_dependencies",
                "به دلیل وجود اطلاعات وابسته، حذف رشته امکان‌پذیر نیست.");
        }
    }
}
