using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class EducationExpertProfileConfiguration : IEntityTypeConfiguration<EducationExpertProfile>
{
    public void Configure(EntityTypeBuilder<EducationExpertProfile> builder)
    {
        builder.ToTable("EducationExpertProfiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EmployeeCode).HasMaxLength(50).IsRequired();

        builder.HasIndex(x => x.EmployeeCode).IsUnique();
        builder.HasIndex(x => x.UserProfileId).IsUnique();

        builder.HasOne<UserProfile>()
            .WithOne()
            .HasForeignKey<EducationExpertProfile>(x => x.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}