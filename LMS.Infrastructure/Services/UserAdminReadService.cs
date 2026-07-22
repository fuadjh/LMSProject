using Application.Abstractions.Read;
using Application.Common.Models;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class UserAdminReadService : IUserAdminReadService
{
    private readonly LmsDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserAdminReadService(LmsDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<IReadOnlyCollection<UserLookupDto>> SearchUsersAsync(string? search, CancellationToken cancellationToken = default)
    {
        var query =
            from p in _dbContext.UserProfiles
            join u in _dbContext.Users on p.AuthUserId equals u.Id
            select new { p, u };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(x =>
                x.u.UserName!.Contains(s) ||
                (x.p.FirstName + " " + x.p.LastName).Contains(s));
        }

        return await query
            .OrderBy(x => x.p.FirstName)
            .ThenBy(x => x.p.LastName)
            .Take(50)
            .Select(x => new UserLookupDto(
                x.p.Id,
                x.u.Id,
                x.p.FirstName + " " + x.p.LastName,
                x.u.UserName!,
                x.p.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<UserAccessDetailsDto?> GetUserAccessDetailsAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        var data = await (
            from p in _dbContext.UserProfiles
            join u in _dbContext.Users on p.AuthUserId equals u.Id
            where p.Id == userProfileId
            select new
            {
                p.Id,
                AuthUserId = u.Id,
                FullName = p.FirstName + " " + p.LastName,
                UserName = u.UserName!
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (data is null)
            return null;

        var user = await _userManager.FindByIdAsync(data.AuthUserId.ToString());
        var roles = user is null ? Array.Empty<string>() : await _userManager.GetRolesAsync(user);

        var facultyIds = await _dbContext.Set<Domain.Entities.Users.UserFacultyScope>()
            .Where(x => x.UserProfileId == userProfileId)
            .Select(x => x.FacultyId)
            .ToListAsync(cancellationToken);

        var majorIds = await _dbContext.Set<Domain.Entities.Users.UserMajorScope>()
            .Where(x => x.UserProfileId == userProfileId)
            .Select(x => x.MajorId)
            .ToListAsync(cancellationToken);

        return new UserAccessDetailsDto(
            data.Id,
            data.AuthUserId,
            data.FullName,
            data.UserName,
            roles.ToArray(),
            facultyIds,
            majorIds);
    }
}