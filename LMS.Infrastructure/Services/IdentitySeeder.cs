
using Common.Security;
using Domain.Entities.Users;
using Infrastructure.Identity;
using Infrastructure.Persistence;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Infrastructure.Services;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<LmsDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await dbContext.Database.MigrateAsync();

        foreach (var roleName in RoleNames.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new ApplicationRole
                {
                    Id = Guid.NewGuid(),
                    Name = roleName
                });

                if (!result.Succeeded)
                    throw new InvalidOperationException(string.Join(" | ", result.Errors.Select(x => x.Description)));
            }
        }

        await SetRolePermissionsAsync(roleManager, RoleNames.Admin, Permissions.All);

        await SetRolePermissionsAsync(roleManager, RoleNames.EducationExpert, new[]
        {
    Permissions.ReferenceData.View,
    Permissions.ReferenceData.Manage,

    Permissions.Courses.View,
    Permissions.Courses.Manage,

    Permissions.Semesters.View,
    Permissions.Semesters.Manage,

    Permissions.CourseOfferings.Create,
    Permissions.CourseOfferings.View,
    Permissions.CourseOfferings.AssignInstructor,
    Permissions.CourseOfferings.EnrollStudent,
    Permissions.CourseOfferings.ManageEnrollment,

    Permissions.Students.Create,
    Permissions.Students.Edit,
    Permissions.Students.View,

    Permissions.Instructors.Create,
    Permissions.Instructors.Edit,
    Permissions.Instructors.View,

    Permissions.EducationExperts.View
});

        await SetRolePermissionsAsync(roleManager, RoleNames.Instructor, new[]
        {
    Permissions.Courses.View,
    Permissions.Semesters.View,
    Permissions.CourseOfferings.View,

    Permissions.QuestionBank.View,
    Permissions.QuestionBank.Manage,

    Permissions.Exams.View,
    Permissions.Exams.Manage,
    Permissions.Exams.Grade
});

        await SetRolePermissionsAsync(roleManager, RoleNames.Student, new[]
        {
    Permissions.Courses.View,
    Permissions.Semesters.View,
    Permissions.CourseOfferings.View,

    Permissions.Exams.View,
    Permissions.Exams.Take
});

        var adminUser = await userManager.FindByNameAsync("admin");
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "admin",
                Email = "admin@lms.local",
                EmailConfirmed = true,
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(adminUser, "Admin@123456");
            if (!createResult.Succeeded)
                throw new InvalidOperationException(string.Join(" | ", createResult.Errors.Select(x => x.Description)));

            var addRoleResult = await userManager.AddToRoleAsync(adminUser, RoleNames.Admin);
            if (!addRoleResult.Succeeded)
                throw new InvalidOperationException(string.Join(" | ", addRoleResult.Errors.Select(x => x.Description)));

            var profileExists = await dbContext.UserProfiles.AnyAsync(x => x.AuthUserId == adminUser.Id);
            if (!profileExists)
            {
                var profile = UserProfile.Create(adminUser.Id, "System", "Admin");
                await dbContext.AddAsync(profile);
                await dbContext.SaveChangesAsync();
            }
        }
    }

    private static async Task SetRolePermissionsAsync(
        RoleManager<ApplicationRole> roleManager,
        string roleName,
        IEnumerable<string> permissions)
    {
        var role = await roleManager.FindByNameAsync(roleName)
                   ?? throw new InvalidOperationException($"Role '{roleName}' not found.");

        var currentClaims = await roleManager.GetClaimsAsync(role);
        var currentPermissions = currentClaims
            .Where(x => x.Type == CustomClaimTypes.Permission)
            .Select(x => x.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var permission in permissions.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!currentPermissions.Contains(permission))
            {
                var result = await roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, permission));
                if (!result.Succeeded)
                    throw new InvalidOperationException(string.Join(" | ", result.Errors.Select(x => x.Description)));
            }
        }
    }
}