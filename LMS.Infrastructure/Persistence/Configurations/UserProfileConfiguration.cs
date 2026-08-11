using Domain.Entities.Users;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class UserProfileConfiguration
    : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasIndex(x => x.AuthUserId)
            .IsUnique();

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<UserProfile>(x => x.AuthUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.FacultyScopes)
            .WithOne()
            .HasForeignKey(x => x.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.MajorScopes)
            .WithOne()
            .HasForeignKey(x => x.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.FacultyScopes)
            .HasField("_facultyScopes")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.MajorScopes)
            .HasField("_majorScopes")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}