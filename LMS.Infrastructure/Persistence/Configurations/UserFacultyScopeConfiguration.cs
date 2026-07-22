using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class UserFacultyScopeConfiguration : IEntityTypeConfiguration<UserFacultyScope>
{
    public void Configure(EntityTypeBuilder<UserFacultyScope> builder)
    {
        builder.ToTable("UserFacultyScopes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserProfileId).IsRequired();
        builder.Property(x => x.FacultyId).IsRequired();

        builder.HasIndex(x => new { x.UserProfileId, x.FacultyId }).IsUnique();
    }
}