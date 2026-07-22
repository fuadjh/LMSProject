using Application.Abstractions.Persistence;
using Application.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ReferenceData.Commands.DeleteUniversity;

public sealed class DeleteUniversityCommandHandler
    : IRequestHandler<DeleteUniversityCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteUniversityCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(
        DeleteUniversityCommand request,
        CancellationToken cancellationToken)
    {
        var affectedRows = await _dbContext.Universities
            .Where(x => x.Id == request.Id)
            .ExecuteDeleteAsync(cancellationToken);

        if (affectedRows == 0)
        {
            return Result.NotFound(
                "university.not_found",
                "دانشگاه یافت نشد.");
        }

        return Result.Success();
    }
}