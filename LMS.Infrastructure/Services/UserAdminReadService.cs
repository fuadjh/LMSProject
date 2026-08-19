using Application.Abstractions.Read;
using Application.Common.Models;
using Common.Enums;
using Common.Security;
using Common.Validation;
using Domain.Entities.Users;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class UserAdminReadService
    : IUserAdminReadService
{
    private readonly LmsDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UserAdminReadService(
        LmsDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<PagedResponse<UserListItemDto>> GetUsersAsync(
        UserRoleType roleType,
        string? search,
        int pageNumber,
        int pageSize,
        string? sortBy,
        bool sortDescending,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = BuildUsersQuery(roleType);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();

            query = query.Where(item =>
                item.UserName.Contains(value) ||
                item.FirstName.Contains(value) ||
                item.LastName.Contains(value) ||
                item.FullName.Contains(value) ||
                item.NationalCode.Contains(value) ||
                (item.PhoneNumber != null &&
                 item.PhoneNumber.Contains(value)) ||
                (item.Email != null &&
                 item.Email.Contains(value)) ||
                (item.RoleIdentifier != null &&
                 item.RoleIdentifier.Contains(value)) ||
                (item.MajorTitle != null &&
                 item.MajorTitle.Contains(value)));
        }

        var totalCount =
            await query.CountAsync(cancellationToken);

        query = ApplySort(
            query,
            sortBy,
            sortDescending);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<UserListItemDto>(
            items,
            totalCount);
    }

    public async Task<UserDetailsDto?> GetUserDetailsAsync(
        Guid userId,
        UserRoleType roleType,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty ||
            !Enum.IsDefined(roleType))
        {
            return null;
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == userId,
                cancellationToken);

        if (user is null)
        {
            return null;
        }

        var roleData = await GetRoleDataAsync(
            userId,
            roleType,
            cancellationToken);

        if (roleData is null)
        {
            return null;
        }

        var facultyScopeIds =
            await _dbContext
                .Set<UserFacultyScope>()
                .AsNoTracking()
                .Where(scope =>
                    scope.UserId == userId &&
                    scope.RoleType == roleType)
                .Select(scope => scope.FacultyId)
                .Distinct()
                .ToArrayAsync(cancellationToken);

        var majorScopeIds =
            await _dbContext
                .Set<UserMajorScope>()
                .AsNoTracking()
                .Where(scope =>
                    scope.UserId == userId &&
                    scope.RoleType == roleType)
                .Select(scope => scope.MajorId)
                .Distinct()
                .ToArrayAsync(cancellationToken);

        var security = await GetSecurityDataAsync(userId);

        return new UserDetailsDto(
            user.Id,
            roleType,
            user.UserName ?? string.Empty,
            user.FirstName,
            user.LastName,
            user.NationalCode,
            user.PhoneNumber,
            user.Email,
            user.LatinFirstName,
            user.LatinLastName,
            user.Gender,
            user.ProfileImagePath,
            user.IsActive,
            roleData.RoleIsActive,
            roleData.RoleIdentifier,
            roleData.FacultyId,
            roleData.MajorId,
            roleData.MajorTitle,
            facultyScopeIds,
            majorScopeIds,
            security.Roles,
            security.Permissions);
    }

    public async Task<UserAccessDetailsDto?>
        GetUserAccessDetailsAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return null;
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == userId,
                cancellationToken);

        if (user is null)
        {
            return null;
        }

        var facultyScopeIds =
            await _dbContext
                .Set<UserFacultyScope>()
                .AsNoTracking()
                .Where(scope => scope.UserId == userId)
                .Select(scope => scope.FacultyId)
                .Distinct()
                .ToArrayAsync(cancellationToken);

        var majorScopeIds =
            await _dbContext
                .Set<UserMajorScope>()
                .AsNoTracking()
                .Where(scope => scope.UserId == userId)
                .Select(scope => scope.MajorId)
                .Distinct()
                .ToArrayAsync(cancellationToken);

        var security = await GetSecurityDataAsync(userId);

        return new UserAccessDetailsDto(
            user.Id,
            user.UserName ?? string.Empty,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.IsActive,
            security.Roles,
            security.Permissions,
            facultyScopeIds,
            majorScopeIds);
    }

    public async Task<IReadOnlyCollection<UserLookupDto>>
        SearchUsersAsync(
            string? search,
            CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();

            query = query.Where(user =>
                (user.UserName != null &&
                 user.UserName.Contains(value)) ||
                user.FirstName.Contains(value) ||
                user.LastName.Contains(value) ||
                user.NationalCode.Contains(value) ||
                (user.PhoneNumber != null &&
                 user.PhoneNumber.Contains(value)) ||
                (user.Email != null &&
                 user.Email.Contains(value)));
        }

        return await query
            .OrderBy(user => user.FirstName)
            .ThenBy(user => user.LastName)
            .Take(50)
            .Select(user => new UserLookupDto(
                user.Id,
                user.UserName ?? string.Empty,
                (user.FirstName + " " + user.LastName).Trim(),
                user.NationalCode,
                user.PhoneNumber))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<UserByNationalCodeDto?>
        GetUserByNationalCodeAsync(
            string nationalCode,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nationalCode))
        {
            return null;
        }

        var normalizedNationalCode =
            IranianIdentityNormalizer.NormalizeNationalCode(
                nationalCode);

        return await _dbContext.Users
            .AsNoTracking()
            .Where(user =>
                user.NationalCode == normalizedNationalCode)
            .Select(user => new UserByNationalCodeDto(
                user.Id,
                user.UserName ?? string.Empty,
                user.FirstName,
                user.LastName,
                user.NationalCode,
                user.PhoneNumber,
                user.Email,
                user.IsActive,
                _dbContext.StudentProfiles.Any(
                    profile => profile.Id == user.Id),
                _dbContext.InstructorProfiles.Any(
                    profile => profile.Id == user.Id),
                _dbContext.ExpertProfiles.Any(
                    profile => profile.Id == user.Id)))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private IQueryable<UserListItemDto> BuildUsersQuery(
        UserRoleType roleType)
    {
        return roleType switch
        {
            UserRoleType.Student => BuildStudentsQuery(),

            UserRoleType.Instructor => BuildInstructorsQuery(),

            UserRoleType.Expert => BuildExpertsQuery(),

            _ => throw new ArgumentOutOfRangeException(
                nameof(roleType),
                roleType,
                "نوع کاربر معتبر نیست.")
        };
    }

    private IQueryable<UserListItemDto> BuildStudentsQuery()
    {
        return
            from user in _dbContext.Users.AsNoTracking()
            join student in
                _dbContext.StudentProfiles.AsNoTracking()
                on user.Id equals student.Id
            join major in
                _dbContext.Majors.AsNoTracking()
                on student.MajorId equals major.Id
            select new UserListItemDto(
                user.Id,
                UserRoleType.Student,
                user.UserName ?? string.Empty,
                user.FirstName,
                user.LastName,
                user.FirstName + " " + user.LastName,
                user.NationalCode,
                user.PhoneNumber,
                user.Email,
                user.IsActive,
                student.IsActive,
                student.StudentNumber,
                major.FacultyId,
                major.Id,
                major.Title);
    }

    private IQueryable<UserListItemDto> BuildInstructorsQuery()
    {
        return
            from user in _dbContext.Users.AsNoTracking()
            join instructor in
                _dbContext.InstructorProfiles.AsNoTracking()
                on user.Id equals instructor.Id
            select new UserListItemDto(
                user.Id,
                UserRoleType.Instructor,
                user.UserName ?? string.Empty,
                user.FirstName,
                user.LastName,
                user.FirstName + " " + user.LastName,
                user.NationalCode,
                user.PhoneNumber,
                user.Email,
                user.IsActive,
                instructor.IsActive,
                instructor.PersonnelCode,
                null,
                null,
                null);
    }

    private IQueryable<UserListItemDto> BuildExpertsQuery()
    {
        return
            from user in _dbContext.Users.AsNoTracking()
            join expert in
                _dbContext.ExpertProfiles.AsNoTracking()
                on user.Id equals expert.Id
            select new UserListItemDto(
                user.Id,
                UserRoleType.Expert,
                user.UserName ?? string.Empty,
                user.FirstName,
                user.LastName,
                user.FirstName + " " + user.LastName,
                user.NationalCode,
                user.PhoneNumber,
                user.Email,
                user.IsActive,
                expert.IsActive,
                null,
                null,
                null,
                null);
    }

    private static IQueryable<UserListItemDto> ApplySort(
        IQueryable<UserListItemDto> query,
        string? sortBy,
        bool descending)
    {
        var normalizedSort = sortBy?
            .Trim()
            .ToLowerInvariant();

        return (normalizedSort, descending) switch
        {
            ("username", true) =>
                query.OrderByDescending(item => item.UserName),

            ("username", false) =>
                query.OrderBy(item => item.UserName),

            ("nationalcode", true) =>
                query.OrderByDescending(item => item.NationalCode),

            ("nationalcode", false) =>
                query.OrderBy(item => item.NationalCode),

            ("identifier", true) =>
                query.OrderByDescending(item => item.RoleIdentifier),

            ("identifier", false) =>
                query.OrderBy(item => item.RoleIdentifier),

            ("major", true) =>
                query.OrderByDescending(item => item.MajorTitle),

            ("major", false) =>
                query.OrderBy(item => item.MajorTitle),

            ("fullname", true) =>
                query.OrderByDescending(item => item.FullName),

            ("fullname", false) =>
                query.OrderBy(item => item.FullName),

            (_, true) =>
                query.OrderByDescending(item => item.LastName)
                    .ThenByDescending(item => item.FirstName),

            _ =>
                query.OrderBy(item => item.LastName)
                    .ThenBy(item => item.FirstName)
        };
    }

    private async Task<UserRoleReadData?> GetRoleDataAsync(
        Guid userId,
        UserRoleType roleType,
        CancellationToken cancellationToken)
    {
        switch (roleType)
        {
            case UserRoleType.Student:
                {
                    return await (
                        from student in
                            _dbContext.StudentProfiles.AsNoTracking()
                        join major in
                            _dbContext.Majors.AsNoTracking()
                            on student.MajorId equals major.Id
                        where student.Id == userId
                        select new UserRoleReadData(
                            student.IsActive,
                            student.StudentNumber,
                            major.FacultyId,
                            major.Id,
                            major.Title))
                        .SingleOrDefaultAsync(cancellationToken);
                }

            case UserRoleType.Instructor:
                {
                    return await _dbContext.InstructorProfiles
                        .AsNoTracking()
                        .Where(profile => profile.Id == userId)
                        .Select(profile => new UserRoleReadData(
                            profile.IsActive,
                            profile.PersonnelCode,
                            null,
                            null,
                            null))
                        .SingleOrDefaultAsync(cancellationToken);
                }

            case UserRoleType.Expert:
                {
                    return await _dbContext.ExpertProfiles
                        .AsNoTracking()
                        .Where(profile => profile.Id == userId)
                        .Select(profile => new UserRoleReadData(
                            profile.IsActive,
                            null,
                            null,
                            null,
                            null))
                        .SingleOrDefaultAsync(cancellationToken);
                }

            default:
                return null;
        }
    }

    private async Task<UserSecurityData> GetSecurityDataAsync(
        Guid userId)
    {
        var user = await _userManager.FindByIdAsync(
            userId.ToString());

        if (user is null)
        {
            return new UserSecurityData(
                Array.Empty<string>(),
                Array.Empty<string>());
        }

        var roles = (await _userManager.GetRolesAsync(user))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var permissions = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var roleName in roles)
        {
            var role =
                await _roleManager.FindByNameAsync(roleName);

            if (role is null)
            {
                continue;
            }

            var claims =
                await _roleManager.GetClaimsAsync(role);

            foreach (var claim in claims.Where(claim =>
                         claim.Type ==
                         CustomClaimTypes.Permission))
            {
                permissions.Add(claim.Value);
            }
        }

        return new UserSecurityData(
            roles,
            permissions
                .OrderBy(permission => permission)
                .ToArray());
    }

    private sealed record UserRoleReadData(
        bool RoleIsActive,
        string? RoleIdentifier,
        Guid? FacultyId,
        Guid? MajorId,
        string? MajorTitle);

    private sealed record UserSecurityData(
        IReadOnlyCollection<string> Roles,
        IReadOnlyCollection<string> Permissions);
}