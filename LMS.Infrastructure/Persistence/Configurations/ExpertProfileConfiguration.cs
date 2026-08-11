using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ExpertProfileConfiguration
    : IEntityTypeConfiguration<ExpertProfile>
{
    public void Configure(EntityTypeBuilder<ExpertProfile> builder)
    {
        builder.ToTable("ExpertProfiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.HasIndex(x => x.UserProfileId)
            .IsUnique();

        builder.HasOne<UserProfile>()
            .WithOne()
            .HasForeignKey<ExpertProfile>(x => x.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}