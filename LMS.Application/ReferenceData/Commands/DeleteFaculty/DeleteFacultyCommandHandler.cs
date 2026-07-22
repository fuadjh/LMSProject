using Application.Abstractions.Persistence;
using Application.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ReferenceData.Commands.DeleteFaculty;

public sealed class DeleteFacultyCommandHandler
    : IRequestHandler<DeleteFacultyCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteFacultyCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(
        DeleteFacultyCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var affectedRows = await _dbContext.Faculties
                .Where(x => x.Id == request.Id)
                .ExecuteDeleteAsync(cancellationToken);

            if (affectedRows == 0)
            {
                return Result.NotFound(
                    "faculty.not_found",
                    "دانشکده یافت نشد.");
            }

            return Result.Success();
        }
        catch (DbUpdateException)
        {
            return Result.Conflict(
                "faculty.has_dependencies",
                "به دلیل وجود اطلاعات وابسته، حذف دانشکده امکان‌پذیر نیست.");
        }
    }
}