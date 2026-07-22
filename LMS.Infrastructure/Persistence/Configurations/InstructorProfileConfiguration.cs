using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class InstructorProfileConfiguration : IEntityTypeConfiguration<InstructorProfile>
{
    public void Configure(EntityTypeBuilder<InstructorProfile> builder)
    {
        builder.ToTable("InstructorProfiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PersonnelCode).HasMaxLength(50).IsRequired();

        builder.HasIndex(x => x.PersonnelCode).IsUnique();
        builder.HasIndex(x => x.UserProfileId).IsUnique();

        builder.HasOne<UserProfile>()
            .WithOne()
            .HasForeignKey<InstructorProfile>(x => x.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}