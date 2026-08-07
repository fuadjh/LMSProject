using Domain.Entities.Users;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ExpertProfileConfiguration
    : IEntityTypeConfiguration<ExpertProfile>
{
    public void Configure(
        EntityTypeBuilder<ExpertProfile> builder)
    {
        builder.ToTable("ExpertProfiles");

        builder.HasKey(profile => profile.Id);

        builder.Property(profile => profile.Id)
            .ValueGeneratedNever();

        builder.Property(profile => profile.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasOne<ApplicationUser>()
            .WithOne(user => user.ExpertProfile)
            .HasForeignKey<ExpertProfile>(profile => profile.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}