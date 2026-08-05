using Application.Abstractions.Read;
using Application.Common.Models;
using Domain.Entities.Users;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class UserAdminReadService : IUserAdminReadService
{
    private readonly LmsDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserAdminReadService(
        LmsDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<IReadOnlyCollection<UserLookupDto>> SearchUsersAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query =
            from profile in _dbContext.UserProfiles.AsNoTracking()
            join user in _dbContext.Users.AsNoTracking()
                on profile.AuthUserId equals user.Id
            select new
            {
                Profile = profile,
                User = user
            };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();

            query = query.Where(x =>
                (x.User.UserName != null &&
                 x.User.UserName.Contains(value)) ||
                (x.User.Email != null &&
                 x.User.Email.Contains(value)) ||
                (x.Profile.NationalCode != null &&
                 x.Profile.NationalCode.Contains(value)) ||
                (x.Profile.FirstName + " " + x.Profile.LastName)
                    .Contains(value));
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
                x.Profile.IsActive && x.User.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<UserAccessDetailsDto?> GetUserAccessDetailsAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        var data = await (
                from profile in _dbContext.UserProfiles.AsNoTracking()
                join user in _dbContext.Users.AsNoTracking()
                    on profile.AuthUserId equals user.Id
                where profile.Id == userProfileId
                select new
                {
                    profile.Id,
                    AuthUserId = user.Id,
                    FullName = profile.FirstName + " " + profile.LastName,
                    UserName = user.UserName ?? string.Empty
                })
            .SingleOrDefaultAsync(cancellationToken);

        if (data is null)
            return null;

        var roles = await GetRolesAsync(data.AuthUserId);

        var facultyIds = await _dbContext
            .Set<UserFacultyScope>()
            .AsNoTracking()
            .Where(x => x.UserProfileId == userProfileId)
            .Select(x => x.FacultyId)
            .ToListAsync(cancellationToken);

        var majorIds = await _dbContext
            .Set<UserMajorScope>()
            .AsNoTracking()
            .Where(x => x.UserProfileId == userProfileId)
            .Select(x => x.MajorId)
            .ToListAsync(cancellationToken);

        return new UserAccessDetailsDto(
            data.Id,
            data.AuthUserId,
            data.FullName,
            data.UserName,
            roles,
            facultyIds,
            majorIds);
    }

    public async Task<PagedResponse<UserProfileListItemDto>> GetUsersAsync(
        UserProfileType profileType,
        string? search,
        int pageNumber,
        int pageSize,
        string? sortBy,
        bool sortDescending,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = BuildProfileQuery(profileType);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();

            query = query.Where(x =>
                x.FirstName.Contains(value) ||
                x.LastName.Contains(value) ||
                (x.FirstName + " " + x.LastName).Contains(value) ||
                x.UserName.Contains(value) ||
                x.Email.Contains(value) ||
                x.ProfileCode.Contains(value) ||
                (x.NationalCode != null &&
                 x.NationalCode.Contains(value)) ||
                (x.MajorTitle != null &&
                 x.MajorTitle.Contains(value)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplySort(
            query,
            sortBy,
            sortDescending);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserProfileListItemDto(
                x.UserProfileId,
                x.AuthUserId,
                x.NationalCode,
                x.UserName,
                x.Email,
                x.FirstName + " " + x.LastName,
                x.ProfileCode,
                x.MajorTitle,
                x.IsActive))
            .ToListAsync(cancellationToken);

        return new PagedResponse<UserProfileListItemDto>(
            items,
            totalCount);
    }

    public async Task<UserProfileDetailsDto?>
        GetUserProfileDetailsAsync(
            Guid userProfileId,
            UserProfileType profileType,
            CancellationToken cancellationToken = default)
    {
        var data = await BuildProfileQuery(profileType)
            .SingleOrDefaultAsync(
                x => x.UserProfileId == userProfileId,
                cancellationToken);

        if (data is null)
            return null;

        var roles = await GetRolesAsync(data.AuthUserId);

        var facultyIds = await _dbContext
            .Set<UserFacultyScope>()
            .AsNoTracking()
            .Where(x => x.UserProfileId == userProfileId)
            .Select(x => x.FacultyId)
            .ToListAsync(cancellationToken);

        var majorIds = await _dbContext
            .Set<UserMajorScope>()
            .AsNoTracking()
            .Where(x => x.UserProfileId == userProfileId)
            .Select(x => x.MajorId)
            .ToListAsync(cancellationToken);

        return new UserProfileDetailsDto(
            data.UserProfileId,
            data.AuthUserId,
            data.NationalCode,
            data.UserName,
            data.Email,
            data.FirstName,
            data.LastName,
            data.IsActive,
            profileType,
            data.ProfileCode,
            data.MajorId,
            data.FacultyId,
            data.MajorTitle,
            roles,
            facultyIds,
            majorIds);
    }

    public async Task<UserByNationalCodeDto?>
        GetUserByNationalCodeAsync(
            string nationalCode,
            CancellationToken cancellationToken = default)
    {
        var normalizedNationalCode =
            UserProfile.NormalizeNationalCode(nationalCode);

        return await (
                from profile in _dbContext.UserProfiles.AsNoTracking()
                join user in _dbContext.Users.AsNoTracking()
                    on profile.AuthUserId equals user.Id
                where profile.NationalCode == normalizedNationalCode
                select new UserByNationalCodeDto(
                    profile.Id,
                    profile.AuthUserId,
                    profile.NationalCode!,
                    profile.FirstName + " " + profile.LastName,
                    user.UserName ?? string.Empty,
                    user.Email ?? string.Empty,
                    profile.IsActive && user.IsActive,
                    _dbContext.StudentProfiles.Any(
                        x => x.UserProfileId == profile.Id),
                    _dbContext.InstructorProfiles.Any(
                        x => x.UserProfileId == profile.Id),
                    _dbContext.EducationExpertProfiles.Any(
                        x => x.UserProfileId == profile.Id)))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private IQueryable<UserRow> BuildProfileQuery(
        UserProfileType profileType)
    {
        return profileType switch
        {
            UserProfileType.Student =>
                from profile in _dbContext.UserProfiles.AsNoTracking()
                join user in _dbContext.Users.AsNoTracking()
                    on profile.AuthUserId equals user.Id
                join student in _dbContext.StudentProfiles.AsNoTracking()
                    on profile.Id equals student.UserProfileId
                join major in _dbContext.Majors.AsNoTracking()
                    on student.MajorId equals major.Id
                select new UserRow(
                    profile.Id,
                    profile.AuthUserId,
                    profile.NationalCode,
                    user.UserName ?? string.Empty,
                    user.Email ?? string.Empty,
                    profile.FirstName,
                    profile.LastName,
                    student.StudentNumber,
                    student.MajorId,
                    major.FacultyId,
                    major.Title,
                    profile.IsActive && user.IsActive),

            UserProfileType.Instructor =>
                from profile in _dbContext.UserProfiles.AsNoTracking()
                join user in _dbContext.Users.AsNoTracking()
                    on profile.AuthUserId equals user.Id
                join instructor in
                    _dbContext.InstructorProfiles.AsNoTracking()
                    on profile.Id equals instructor.UserProfileId
                select new UserRow(
                    profile.Id,
                    profile.AuthUserId,
                    profile.NationalCode,
                    user.UserName ?? string.Empty,
                    user.Email ?? string.Empty,
                    profile.FirstName,
                    profile.LastName,
                    instructor.PersonnelCode,
                    null,
                    null,
                    null,
                    profile.IsActive && user.IsActive),

            UserProfileType.EducationExpert =>
                from profile in _dbContext.UserProfiles.AsNoTracking()
                join user in _dbContext.Users.AsNoTracking()
                    on profile.AuthUserId equals user.Id
                join expert in
                    _dbContext.EducationExpertProfiles.AsNoTracking()
                    on profile.Id equals expert.UserProfileId
                select new UserRow(
                    profile.Id,
                    profile.AuthUserId,
                    profile.NationalCode,
                    user.UserName ?? string.Empty,
                    user.Email ?? string.Empty,
                    profile.FirstName,
                    profile.LastName,
                    expert.EmployeeCode,
                    null,
                    null,
                    null,
                    profile.IsActive && user.IsActive),

            _ => throw new ArgumentOutOfRangeException(
                nameof(profileType),
                profileType,
                "نوع پروفایل پشتیبانی نمی‌شود.")
        };
    }

    private static IQueryable<UserRow> ApplySort(
        IQueryable<UserRow> query,
        string? sortBy,
        bool descending)
    {
        var normalizedSort = sortBy?
            .Trim()
            .ToLowerInvariant();

        return (normalizedSort, descending) switch
        {
            ("username", true) =>
                query.OrderByDescending(x => x.UserName),

            ("username", false) =>
                query.OrderBy(x => x.UserName),

            ("nationalcode", true) =>
                query.OrderByDescending(x => x.NationalCode),

            ("nationalcode", false) =>
                query.OrderBy(x => x.NationalCode),

            ("profilecode", true) =>
                query.OrderByDescending(x => x.ProfileCode),

            ("profilecode", false) =>
                query.OrderBy(x => x.ProfileCode),

            ("major", true) =>
                query.OrderByDescending(x => x.MajorTitle),

            ("major", false) =>
                query.OrderBy(x => x.MajorTitle),

            (_, true) =>
                query.OrderByDescending(x => x.LastName)
                    .ThenByDescending(x => x.FirstName),

            _ =>
                query.OrderBy(x => x.LastName)
                    .ThenBy(x => x.FirstName)
        };
    }

    private async Task<IReadOnlyCollection<string>> GetRolesAsync(
        Guid authUserId)
    {
        var user = await _userManager.FindByIdAsync(
            authUserId.ToString());

        if (user is null)
            return Array.Empty<string>();

        var roles = await _userManager.GetRolesAsync(user);
        return roles.ToArray();
    }

    private sealed record UserRow(
        Guid UserProfileId,
        Guid AuthUserId,
        string? NationalCode,
        string UserName,
        string Email,
        string FirstName,
        string LastName,
        string ProfileCode,
        Guid? MajorId,
        Guid? FacultyId,
        string? MajorTitle,
        bool IsActive);
}