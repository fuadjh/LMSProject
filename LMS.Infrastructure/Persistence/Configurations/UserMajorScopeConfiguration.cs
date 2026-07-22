using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class UserMajorScopeConfiguration : IEntityTypeConfiguration<UserMajorScope>
{
    public void Configure(EntityTypeBuilder<UserMajorScope> builder)
    {
        builder.ToTable("UserMajorScopes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserProfileId).IsRequired();
        builder.Property(x => x.MajorId).IsRequired();

        builder.HasIndex(x => new { x.UserProfileId, x.MajorId }).IsUnique();
    }
}