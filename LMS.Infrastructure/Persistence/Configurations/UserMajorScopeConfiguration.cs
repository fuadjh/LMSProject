using Domain.Entities.Academics;
using Domain.Entities.Users;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class UserMajorScopeConfiguration
    : IEntityTypeConfiguration<UserMajorScope>
{
    public void Configure(
        EntityTypeBuilder<UserMajorScope> builder)
    {
        builder.ToTable("UserMajorScopes");

        builder.HasKey(scope => scope.Id);

        builder.Property(scope => scope.RoleType)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(scope => new
        {
            scope.UserId,
            scope.RoleType,
            scope.MajorId
        })
            .IsUnique()
            .HasDatabaseName(
                "UX_UserMajorScopes_User_Role_Major");

        builder.HasOne<ApplicationUser>()
            .WithMany(user => user.MajorScopes)
            .HasForeignKey(scope => scope.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Major>()
            .WithMany()
            .HasForeignKey(scope => scope.MajorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasCheckConstraint(
            "CK_UserMajorScopes_RoleType",
            "[RoleType] IN (1, 2, 3)");
    }
}