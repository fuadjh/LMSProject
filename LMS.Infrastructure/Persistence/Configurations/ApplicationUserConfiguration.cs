using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ApplicationUserConfiguration
    : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(
        EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.NationalCode)
            .HasMaxLength(10)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(user => user.PhoneNumber)
            .HasMaxLength(11)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(user => user.LatinFirstName)
            .HasMaxLength(100)
            .IsUnicode(false);

        builder.Property(user => user.LatinLastName)
            .HasMaxLength(100)
            .IsUnicode(false);

        builder.Property(user => user.ProfileImagePath)
            .HasMaxLength(500);

        builder.Property(user => user.Gender)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(user => user.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(user => user.NationalCode)
            .IsUnique()
            .HasDatabaseName("UX_AspNetUsers_NationalCode");

        builder.HasIndex(user => user.PhoneNumber)
            .IsUnique()
            .HasDatabaseName("UX_AspNetUsers_PhoneNumber");

        builder.HasCheckConstraint(
            "CK_AspNetUsers_NationalCode",
            "[NationalCode] LIKE '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'");

        builder.HasCheckConstraint(
            "CK_AspNetUsers_PhoneNumber",
            "[PhoneNumber] LIKE '09[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'");

        builder.HasCheckConstraint(
            "CK_AspNetUsers_Gender",
            "[Gender] IN (1, 2)");
    }
}