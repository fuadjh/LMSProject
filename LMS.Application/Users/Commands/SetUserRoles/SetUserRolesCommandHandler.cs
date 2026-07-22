using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;

namespace Application.Users.Commands.SetUserRoles;

public sealed class SetUserRolesCommandHandler
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _dbContext;

    public SetUserRolesCommandHandler(
        IIdentityService identityService,
        IApplicationDbContext dbContext)
    {
        _identityService = identityService;
        _dbContext = dbContext;
    }

    public async Task Handle(SetUserRolesCommand request, CancellationToken cancellationToken)
    {
        await using var tx = await _dbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            await _identityService.SetUserRolesAsync(
                request.AuthUserId,
                request.Roles,
                cancellationToken);

            await tx.CommitAsync(cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }
}