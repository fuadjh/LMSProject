using Domain.Entities.Users;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class InstructorProfileConfiguration
    : IEntityTypeConfiguration<InstructorProfile>
{
    public void Configure(
        EntityTypeBuilder<InstructorProfile> builder)
    {
        builder.ToTable("InstructorProfiles");

        builder.HasKey(profile => profile.Id);

        builder.Property(profile => profile.Id)
            .ValueGeneratedNever();

        builder.Property(profile => profile.PersonnelCode)
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(profile => profile.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(profile => profile.PersonnelCode)
            .IsUnique()
            .HasDatabaseName("UX_InstructorProfiles_PersonnelCode");

        builder.HasOne<ApplicationUser>()
            .WithOne(user => user.InstructorProfile)
            .HasForeignKey<InstructorProfile>(profile => profile.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}