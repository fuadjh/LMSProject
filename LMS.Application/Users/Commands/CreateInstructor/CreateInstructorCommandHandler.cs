using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Common.Security;
using Domain.Entities.Users;

using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.CreateInstructor;

public sealed class CreateInstructorCommandHandler
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _dbContext;

    public CreateInstructorCommandHandler(
        IIdentityService identityService,
        IApplicationDbContext dbContext)
    {
        _identityService = identityService;
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateInstructorCommand request, CancellationToken cancellationToken)
    {
        if (await _identityService.ExistsByUserNameAsync(request.UserName, cancellationToken))
            throw new InvalidOperationException("Username already exists.");

        if (await _identityService.ExistsByEmailAsync(request.Email, cancellationToken))
            throw new InvalidOperationException("Email already exists.");

        var personnelCodeExists = await _dbContext.InstructorProfiles
            .AnyAsync(x => x.PersonnelCode == request.PersonnelCode, cancellationToken);

        if (personnelCodeExists)
            throw new InvalidOperationException("Personnel code already exists.");

        await using var tx = await _dbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            var authUserId = await _identityService.CreateUserAsync(
                request.UserName,
                request.Email,
                request.Password,
                new[] { RoleNames.Instructor },
                cancellationToken);

            var userProfile = UserProfile.Create(authUserId, request.FirstName, request.LastName);
            var instructorProfile = InstructorProfile.Create(userProfile.Id, request.PersonnelCode);

            await _dbContext.AddAsync(userProfile, cancellationToken);
            await _dbContext.AddAsync(instructorProfile, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await tx.CommitAsync(cancellationToken);

            return userProfile.Id;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }
}