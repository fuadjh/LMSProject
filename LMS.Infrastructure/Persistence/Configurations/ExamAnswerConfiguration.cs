using Domain.Entities.Exams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ExamAnswerConfiguration : IEntityTypeConfiguration<ExamAnswer>
{
    public void Configure(EntityTypeBuilder<ExamAnswer> builder)
    {
        builder.ToTable("ExamAnswers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ExamSubmissionId).IsRequired();
        builder.Property(x => x.QuestionId).IsRequired();
        builder.Property(x => x.EssayAttachmentFileName).HasMaxLength(260);
        builder.Property(x => x.EssayAttachmentPath).HasMaxLength(500);
        builder.Property(x => x.AwardedScore).HasPrecision(8, 2);

        builder.HasIndex(x => new { x.ExamSubmissionId, x.QuestionId }).IsUnique();
    }
}