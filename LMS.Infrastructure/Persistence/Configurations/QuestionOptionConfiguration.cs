using Domain.Entities.Exams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> builder)
    {
        builder.ToTable("QuestionOptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuestionId).IsRequired();
        builder.Property(x => x.Text).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Order).IsRequired();
        builder.Property(x => x.IsCorrect).IsRequired();

        builder.HasIndex(x => new { x.QuestionId, x.Order }).IsUnique();
    }
}