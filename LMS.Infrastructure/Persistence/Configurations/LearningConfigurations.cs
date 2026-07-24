using Domain.Entities.Learning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class LearningModuleConfiguration
    : IEntityTypeConfiguration<LearningModule>
{
    public void Configure(
        EntityTypeBuilder<LearningModule> builder)
    {
        builder.ToTable("LearningModules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.Order)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.CourseOfferingId,
            x.Order
        }).IsUnique();
    }
}

public sealed class LearningItemConfiguration
    : IEntityTypeConfiguration<LearningItem>
{
    public void Configure(
        EntityTypeBuilder<LearningItem> builder)
    {
        builder.ToTable("LearningItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ExternalUrl)
            .HasMaxLength(2000);

        builder.Property(x => x.StorageKey)
            .HasMaxLength(1000);

        builder.Property(x => x.HlsMasterPlaylistKey)
            .HasMaxLength(1000);

        builder.Property(x => x.OriginalFileName)
            .HasMaxLength(260);

        builder.Property(x => x.ContentType)
            .HasMaxLength(200);

        builder.Property(x => x.CoverStorageKey)
            .HasMaxLength(1000);

        builder.HasIndex(x => new
        {
            x.LearningModuleId,
            x.Order
        }).IsUnique();
    }
}

public sealed class LearningItemProgressConfiguration
    : IEntityTypeConfiguration<LearningItemProgress>
{
    public void Configure(
        EntityTypeBuilder<LearningItemProgress> builder)
    {
        builder.ToTable("LearningItemProgresses");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new
        {
            x.LearningItemId,
            x.StudentProfileId
        }).IsUnique();
    }
}

public sealed class LearningTemplateConfiguration
    : IEntityTypeConfiguration<LearningTemplate>
{
    public void Configure(
        EntityTypeBuilder<LearningTemplate> builder)
    {
        builder.ToTable("LearningTemplates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.CourseId,
            x.InstructorProfileId,
            x.Title
        }).IsUnique();
    }
}

public sealed class LearningTemplateModuleConfiguration
    : IEntityTypeConfiguration<LearningTemplateModule>
{
    public void Configure(
        EntityTypeBuilder<LearningTemplateModule> builder)
    {
        builder.ToTable("LearningTemplateModules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.HasIndex(x => new
        {
            x.LearningTemplateId,
            x.Order
        }).IsUnique();
    }
}

public sealed class LearningTemplateItemConfiguration
    : IEntityTypeConfiguration<LearningTemplateItem>
{
    public void Configure(
        EntityTypeBuilder<LearningTemplateItem> builder)
    {
        builder.ToTable("LearningTemplateItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ExternalUrl)
            .HasMaxLength(2000);

        builder.Property(x => x.StorageKey)
            .HasMaxLength(1000);

        builder.Property(x => x.HlsMasterPlaylistKey)
            .HasMaxLength(1000);

        builder.Property(x => x.OriginalFileName)
            .HasMaxLength(260);

        builder.Property(x => x.ContentType)
            .HasMaxLength(200);

        builder.Property(x => x.CoverStorageKey)
            .HasMaxLength(1000);

        builder.HasIndex(x => new
        {
            x.LearningTemplateModuleId,
            x.Order
        }).IsUnique();
    }
}