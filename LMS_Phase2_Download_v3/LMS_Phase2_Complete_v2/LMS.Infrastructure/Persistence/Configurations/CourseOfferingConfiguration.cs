using Domain.Entities.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class CourseOfferingConfiguration
    : IEntityTypeConfiguration<CourseOffering>
{
    public void Configure(EntityTypeBuilder<CourseOffering> builder)
    {
        builder.ToTable("CourseOfferings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CourseId).IsRequired();
        builder.Property(x => x.SemesterId).IsRequired();
        builder.Property(x => x.SectionCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Capacity).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.HasIndex(x => new
        {
            x.CourseId, x.SemesterId, x.SectionCode
        }).IsUnique();
    }
}
