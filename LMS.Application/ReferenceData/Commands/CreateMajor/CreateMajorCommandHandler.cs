using Application.Abstractions.Persistence;
using Application.Common.Results;
using Domain.Entities.Academics;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ReferenceData.Commands.CreateMajor;

public sealed class CreateMajorCommandHandler
    : IRequestHandler<CreateMajorCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateMajorCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(
        CreateMajorCommand request,
        CancellationToken cancellationToken)
    {
        var facultyExists = await _dbContext.Faculties
            .AnyAsync(x => x.Id == request.FacultyId && x.IsActive, cancellationToken);

        if (!facultyExists)
        {
            return Result<Guid>.NotFound(
                "faculty.not_found",
                "دانشکده یافت نشد.");
        }

        var codeExists = await _dbContext.Majors
            .AnyAsync(x =>
                x.FacultyId == request.FacultyId &&
                x.Code == request.Code,
                cancellationToken);

        if (codeExists)
        {
            return Result<Guid>.Conflict(
                "major.code_exists",
                "کد رشته تکراری است.");
        }

        var entity = Major.Create(request.FacultyId, request.Title, request.Code);

        await _dbContext.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}