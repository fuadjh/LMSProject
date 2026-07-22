using Application.Abstractions.Persistence;
using Application.Common.Results;
using Domain.Entities.Academics;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ReferenceData.Commands.CreateUniversity;

public sealed class CreateUniversityCommandHandler : IRequestHandler<CreateUniversityCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateUniversityCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(CreateUniversityCommand request, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Universities.AnyAsync(x => x.Code == request.Code, cancellationToken);
        if (exists)
            return Result<Guid>.Conflict("university.code_exists", "کد دانشگاه تکراری است.");

        var entity = University.Create(request.Title, request.Code);

        await _dbContext.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}