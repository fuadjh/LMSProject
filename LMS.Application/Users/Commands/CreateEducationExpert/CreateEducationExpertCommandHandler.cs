using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;

using Common.Security;
using Domain.Entities.Users;

using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.CreateEducationExpert;

public sealed class CreateEducationExpertCommandHandler
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _dbContext;

    public CreateEducationExpertCommandHandler(
        IIdentityService identityService,
        IApplicationDbContext dbContext)
    {
        _identityService = identityService;
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateEducationExpertCommand request, CancellationToken cancellationToken)
    {
        if (await _identityService.ExistsByUserNameAsync(request.UserName, cancellationToken))
            throw new InvalidOperationException("Username already exists.");

        if (await _identityService.ExistsByEmailAsync(request.Email, cancellationToken))
            throw new InvalidOperationException("Email already exists.");

        var employeeCodeExists = await _dbContext.EducationExpertProfiles
            .AnyAsync(x => x.EmployeeCode == request.EmployeeCode, cancellationToken);

        if (employeeCodeExists)
            throw new InvalidOperationException("Employee code already exists.");

        await using var tx = await _dbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            var authUserId = await _identityService.CreateUserAsync(
                request.UserName,
                request.Email,
                request.Password,
                new[] { RoleNames.EducationExpert },
                cancellationToken);

            var userProfile = UserProfile.Create(authUserId, request.FirstName, request.LastName);
            var expertProfile = EducationExpertProfile.Create(userProfile.Id, request.EmployeeCode);

            await _dbContext.AddAsync(userProfile, cancellationToken);
            await _dbContext.AddAsync(expertProfile, cancellationToken);
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