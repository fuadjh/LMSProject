using Domain.Entities.Academics;
using Domain.Entities.Users;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class StudentProfileConfiguration
    : IEntityTypeConfiguration<StudentProfile>
{
    public void Configure(
        EntityTypeBuilder<StudentProfile> builder)
    {
        builder.ToTable("StudentProfiles");

        builder.HasKey(profile => profile.Id);

        builder.Property(profile => profile.Id)
            .ValueGeneratedNever();

        builder.Property(profile => profile.StudentNumber)
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(profile => profile.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(profile => profile.StudentNumber)
            .IsUnique()
            .HasDatabaseName("UX_StudentProfiles_StudentNumber");

        builder.HasOne<ApplicationUser>()
            .WithOne(user => user.StudentProfile)
            .HasForeignKey<StudentProfile>(profile => profile.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Major>()
            .WithMany()
            .HasForeignKey(profile => profile.MajorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}