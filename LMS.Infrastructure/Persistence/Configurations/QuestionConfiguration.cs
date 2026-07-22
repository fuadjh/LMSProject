using Domain.Entities.Exams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("Questions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuestionBankId).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Body).IsRequired();
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.Difficulty).IsRequired();
        builder.Property(x => x.EvaluationDomain).IsRequired();
        builder.Property(x => x.SourceType).IsRequired();
        builder.Property(x => x.SourceDescription).HasMaxLength(500);
        builder.Property(x => x.SuggestedScore).HasPrecision(8, 2).IsRequired();
        builder.Property(x => x.AttachmentFileName).HasMaxLength(260);
        builder.Property(x => x.AttachmentPath).HasMaxLength(500);
        builder.Property(x => x.IsActive).IsRequired();
    }
}