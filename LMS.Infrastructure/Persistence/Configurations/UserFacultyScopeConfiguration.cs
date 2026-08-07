using Domain.Entities.Academics;
using Domain.Entities.Users;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class UserFacultyScopeConfiguration
    : IEntityTypeConfiguration<UserFacultyScope>
{
    public void Configure(
        EntityTypeBuilder<UserFacultyScope> builder)
    {
        builder.ToTable("UserFacultyScopes");

        builder.HasKey(scope => scope.Id);

        builder.Property(scope => scope.RoleType)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(scope => new
        {
            scope.UserId,
            scope.RoleType,
            scope.FacultyId
        })
            .IsUnique()
            .HasDatabaseName(
                "UX_UserFacultyScopes_User_Role_Faculty");

        builder.HasOne<ApplicationUser>()
            .WithMany(user => user.FacultyScopes)
            .HasForeignKey(scope => scope.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Faculty>()
            .WithMany()
            .HasForeignKey(scope => scope.FacultyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasCheckConstraint(
            "CK_UserFacultyScopes_RoleType",
            "[RoleType] IN (1, 2, 3)");
    }
}