using Application.Abstractions.Persistence;
using Application.Common.Results;
using Domain.Entities.Academics;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Semesters.Commands.CreateSemester;

public sealed class CreateSemesterCommandHandler : IRequestHandler<CreateSemesterCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateSemesterCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(CreateSemesterCommand request, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Semesters.AnyAsync(x => x.Title == request.Title, cancellationToken);
        if (exists)
            return Result<Guid>.Conflict("semester.title_exists", "عنوان نیمسال تکراری است.");

        var semester = Semester.Create(request.Title, request.StartsAtUtc, request.EndsAtUtc);

        await _dbContext.AddAsync(semester, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(semester.Id);
    }
}