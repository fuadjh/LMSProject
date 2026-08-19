using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class UserProfileConfiguration
    : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(
        EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");

        builder.HasKey(profile => profile.Id);

        builder.Property(profile => profile.Id)
            .ValueGeneratedNever();

        builder.Property(profile => profile.AuthUserId)
            .IsRequired();

        builder.Property(profile => profile.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(profile => profile.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(profile => profile.NationalCode)
            .HasMaxLength(10)
            .IsUnicode(false);

        builder.Property(profile => profile.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(profile => profile.AuthUserId)
            .IsUnique();

        builder.HasIndex(profile => profile.NationalCode)
            .IsUnique()
            .HasFilter("[NationalCode] IS NOT NULL");
    }
}