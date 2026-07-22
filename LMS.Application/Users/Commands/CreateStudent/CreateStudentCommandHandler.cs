using Application.Abstractions.Auth;
using Application.Abstractions.Persistence;
using Application.Abstractions.Security;
using Common.Security;
using Domain.Entities.Users;

using Application.Users.Commands.CreateStudent;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.CreateStudent;

public sealed class CreateStudentCommandHandler
{
    private readonly ICurrentUser _currentUser;
    private readonly IIdentityService _identityService;
    private readonly IAccessScopeService _accessScopeService;
    private readonly IApplicationDbContext _dbContext;

    public CreateStudentCommandHandler(
        ICurrentUser currentUser,
        IIdentityService identityService,
        IAccessScopeService accessScopeService,
        IApplicationDbContext dbContext)
    {
        _currentUser = currentUser;
        _identityService = identityService;
        _accessScopeService = accessScopeService;
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserProfileId.HasValue)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var majorExists = await _dbContext.Majors
            .AnyAsync(x => x.Id == request.MajorId && x.IsActive, cancellationToken);

        if (!majorExists)
            throw new InvalidOperationException("Major not found.");

        var hasScope = await _accessScopeService.HasMajorAccessAsync(
            _currentUser.UserProfileId.Value,
            request.MajorId,
            cancellationToken);

        if (!hasScope)
            throw new UnauthorizedAccessException("You do not have access to this major.");

        if (await _identityService.ExistsByUserNameAsync(request.UserName, cancellationToken))
            throw new InvalidOperationException("Username already exists.");

        if (await _identityService.ExistsByEmailAsync(request.Email, cancellationToken))
            throw new InvalidOperationException("Email already exists.");

        var studentNumberExists = await _dbContext.StudentProfiles
            .AnyAsync(x => x.StudentNumber == request.StudentNumber, cancellationToken);

        if (studentNumberExists)
            throw new InvalidOperationException("Student number already exists.");

        await using var tx = await _dbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            var authUserId = await _identityService.CreateUserAsync(
                request.UserName,
                request.Email,
                request.Password,
                new[] { RoleNames.Student },
                cancellationToken);

            var userProfile = UserProfile.Create(authUserId, request.FirstName, request.LastName);
            var studentProfile = StudentProfile.Create(userProfile.Id, request.StudentNumber, request.MajorId);

            await _dbContext.AddAsync(userProfile, cancellationToken);
            await _dbContext.AddAsync(studentProfile, cancellationToken);
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