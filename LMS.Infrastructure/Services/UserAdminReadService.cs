using Application.Abstractions.Read;
using Application.Common.Models;
using Common.Enums;
using Common.Security;
using Common.Validation;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class UserAdminReadService(
    LmsDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager)
    : IUserAdminReadService
{
    public async Task<PagedResponse<UserProfileListItemDto>> GetUsersAsync(
        UserRoleType role,
        string? search,
        int page,
        int pageSize,
        string? sortBy,
        bool descending,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var roleName = GetRoleName(role);
        var normalizedRoleName = roleName.ToUpperInvariant();

        var roleIdQuery = dbContext.Roles
            .Where(x => x.NormalizedName == normalizedRoleName)
            .Select(x => x.Id);

        var roleUserIds = dbContext.UserRoles
            .Where(x => roleIdQuery.Contains(x.RoleId))
            .Select(x => x.UserId);

        var query =
            from user in dbContext.Users.AsNoTracking()
            join profile in dbContext.UserProfiles.AsNoTracking()
                on user.Id equals profile.AuthUserId
            where roleUserIds.Contains(user.Id)
            select new
            {
                User = user,
                Profile = profile
            };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();

            query = query.Where(x =>
                x.User.UserName!.Contains(value) ||
                x.User.NationalCode.Contains(value) ||
                x.User.PhoneNumber!.Contains(value) ||
                x.Profile.FirstName.Contains(value) ||
                x.Profile.LastName.Contains(value));
        }

        query = (sortBy?.ToLowerInvariant(), descending) switch
        {
            ("username", true) =>
                query.OrderByDescending(x => x.User.UserName),

            ("username", false) =>
                query.OrderBy(x => x.User.UserName),

            ("nationalcode", true) =>
                query.OrderByDescending(x => x.User.NationalCode),

            ("nationalcode", false) =>
                query.OrderBy(x => x.User.NationalCode),

            (_, true) =>
                query.OrderByDescending(x => x.Profile.FirstName)
                    .ThenByDescending(x => x.Profile.LastName),

            _ =>
                query.OrderBy(x => x.Profile.FirstName)
                    .ThenBy(x => x.Profile.LastName)
        };

        var totalCount =
            await query.CountAsync(cancellationToken);

        var rows = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserProfileListItemDto(
                x.User.Id,
                x.Profile.Id,
                x.User.UserName ?? string.Empty,
                x.Profile.FirstName + " " + x.Profile.LastName,
                x.User.NationalCode,
                x.User.PhoneNumber ?? string.Empty,
                role,
                x.User.IsActive && x.Profile.IsActive,
                role == UserRoleType.Student
                    ? dbContext.StudentProfiles
                        .Where(p => p.UserProfileId == x.Profile.Id)
                        .Select(p => p.StudentNumber)
                        .FirstOrDefault()
                    : null,
                role == UserRoleType.Instructor
                    ? dbContext.InstructorProfiles
                        .Where(p => p.UserProfileId == x.Profile.Id)
                        .Select(p => p.PersonnelCode)
                        .FirstOrDefault()
                    : null))
            .ToListAsync(cancellationToken);

        return new PagedResponse<UserProfileListItemDto>(
            rows,
            page,
            pageSize,
            totalCount);
    }

    public async Task<UserProfileDetailsDto?> GetUserDetailsAsync(
        Guid userId,
        UserRoleType role,
        CancellationToken cancellationToken = default)
    {
        var data = await (
            from user in dbContext.Users.AsNoTracking()
            join profile in dbContext.UserProfiles.AsNoTracking()
                on user.Id equals profile.AuthUserId
            where user.Id == userId
            select new
            {
                User = user,
                Profile = profile
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (data is null)
            return null;

        string? studentNumber = null;
        Guid? studentMajorId = null;
        string? personnelCode = null;

        if (role == UserRoleType.Student)
        {
            var student = await dbContext.StudentProfiles
                .AsNoTracking()
                .Where(x => x.UserProfileId == data.Profile.Id)
                .Select(x => new
                {
                    x.StudentNumber,
                    x.MajorId
                })
                .SingleOrDefaultAsync(cancellationToken);

            studentNumber = student?.StudentNumber;
            studentMajorId = student?.MajorId;
        }

        if (role == UserRoleType.Instructor)
        {
            personnelCode = await dbContext.InstructorProfiles
                .AsNoTracking()
                .Where(x => x.UserProfileId == data.Profile.Id)
                .Select(x => x.PersonnelCode)
                .SingleOrDefaultAsync(cancellationToken);
        }

        var facultyIds = await dbContext
            .Set<Domain.Entities.Users.UserFacultyScope>()
            .AsNoTracking()
            .Where(x => x.UserProfileId == data.Profile.Id)
            .Select(x => x.FacultyId)
            .ToListAsync(cancellationToken);

        var majorIds = await dbContext
            .Set<Domain.Entities.Users.UserMajorScope>()
            .AsNoTracking()
            .Where(x => x.UserProfileId == data.Profile.Id)
            .Select(x => x.MajorId)
            .ToListAsync(cancellationToken);

        return new UserProfileDetailsDto(
            data.User.Id,
            data.Profile.Id,
            data.User.UserName ?? string.Empty,
            data.Profile.FirstName,
            data.Profile.LastName,
            data.User.NationalCode,
            data.User.PhoneNumber ?? string.Empty,
            data.User.Email,
            data.User.LatinFirstName,
            data.User.LatinLastName,
            data.User.Gender,
            data.User.ProfileImagePath,
            data.User.IsActive && data.Profile.IsActive,
            role,
            studentNumber,
            studentMajorId,
            personnelCode,
            facultyIds,
            majorIds);
    }

    public async Task<UserByNationalCodeDto?> GetUserByNationalCodeAsync(
        string nationalCode,
        CancellationToken cancellationToken = default)
    {
        var normalized =
            IranianIdentityNormalizer.NormalizeNationalCode(nationalCode);

        return await (
            from user in dbContext.Users.AsNoTracking()
            join profile in dbContext.UserProfiles.AsNoTracking()
                on user.Id equals profile.AuthUserId
            where user.NationalCode == normalized
            select new UserByNationalCodeDto(
                user.Id,
                user.UserName ?? string.Empty,
                profile.FirstName,
                profile.LastName,
                user.NationalCode,
                user.PhoneNumber,
                user.Email,
                user.IsActive && profile.IsActive,
                dbContext.StudentProfiles.Any(
                    x => x.UserProfileId == profile.Id),
                dbContext.InstructorProfiles.Any(
                    x => x.UserProfileId == profile.Id),
                dbContext.ExpertProfiles.Any(
                    x => x.UserProfileId == profile.Id)))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<UserAccessDetailsDto?> GetUserAccessDetailsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var data = await (
            from user in dbContext.Users.AsNoTracking()
            join profile in dbContext.UserProfiles.AsNoTracking()
                on user.Id equals profile.AuthUserId
            where user.Id == userId
            select new
            {
                User = user,
                Profile = profile
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (data is null)
            return null;

        var identityUser =
            await userManager.FindByIdAsync(userId.ToString());

        if (identityUser is null)
            return null;

        var roles = await userManager.GetRolesAsync(identityUser);
        var permissions = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var roleName in roles)
        {
            var identityRole =
                await roleManager.FindByNameAsync(roleName);

            if (identityRole is null)
                continue;

            var claims =
                await roleManager.GetClaimsAsync(identityRole);

            foreach (var claim in claims.Where(
                         x => x.Type == CustomClaimTypes.Permission))
            {
                permissions.Add(claim.Value);
            }
        }

        var facultyIds = await dbContext
            .Set<Domain.Entities.Users.UserFacultyScope>()
            .AsNoTracking()
            .Where(x => x.UserProfileId == data.Profile.Id)
            .Select(x => x.FacultyId)
            .ToListAsync(cancellationToken);

        var majorIds = await dbContext
            .Set<Domain.Entities.Users.UserMajorScope>()
            .AsNoTracking()
            .Where(x => x.UserProfileId == data.Profile.Id)
            .Select(x => x.MajorId)
            .ToListAsync(cancellationToken);

        return new UserAccessDetailsDto(
            data.User.Id,
            data.Profile.FirstName + " " + data.Profile.LastName,
            data.User.UserName ?? string.Empty,
            data.User.IsActive && data.Profile.IsActive,
            roles.ToArray(),
            permissions.ToArray(),
            facultyIds,
            majorIds);
    }

    public async Task<IReadOnlyCollection<UserLookupDto>> SearchUsersAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query =
            from profile in dbContext.UserProfiles.AsNoTracking()
            join user in dbContext.Users.AsNoTracking()
                on profile.AuthUserId equals user.Id
            select new
            {
                User = user,
                Profile = profile
            };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();

            query = query.Where(x =>
                x.User.UserName!.Contains(value) ||
                x.User.NationalCode.Contains(value) ||
                x.Profile.FirstName.Contains(value) ||
                x.Profile.LastName.Contains(value));
        }

        return await query
            .OrderBy(x => x.Profile.FirstName)
            .ThenBy(x => x.Profile.LastName)
            .Take(50)
            .Select(x => new UserLookupDto(
                x.Profile.Id,
                x.User.Id,
                x.Profile.FirstName + " " + x.Profile.LastName,
                x.User.UserName ?? string.Empty,
                x.User.IsActive && x.Profile.IsActive))
            .ToListAsync(cancellationToken);
    }

    private static string GetRoleName(UserRoleType role) =>
        role switch
        {
            UserRoleType.Student => RoleNames.Student,
            UserRoleType.Instructor => RoleNames.Instructor,
            UserRoleType.EducationExpert => RoleNames.EducationExpert,
            _ => throw new ArgumentOutOfRangeException(
                nameof(role),
                "نوع کاربر معتبر نیست.")
        };
}