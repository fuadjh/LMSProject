using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class StudentProfileConfiguration : IEntityTypeConfiguration<StudentProfile>
{
    public void Configure(EntityTypeBuilder<StudentProfile> builder)
    {
        builder.ToTable("StudentProfiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StudentNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.MajorId).IsRequired();

        builder.HasIndex(x => x.StudentNumber).IsUnique();
        builder.HasIndex(x => x.UserProfileId).IsUnique();

        builder.HasOne<UserProfile>()
            .WithOne()
            .HasForeignKey<StudentProfile>(x => x.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}