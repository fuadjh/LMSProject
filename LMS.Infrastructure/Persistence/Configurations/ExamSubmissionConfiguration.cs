using Domain.Entities.Exams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ExamSubmissionConfiguration : IEntityTypeConfiguration<ExamSubmission>
{
    public void Configure(EntityTypeBuilder<ExamSubmission> builder)
    {
        builder.ToTable("ExamSubmissions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ExamId).IsRequired();
        builder.Property(x => x.StudentProfileId).IsRequired();
        builder.Property(x => x.AttemptNumber).IsRequired();
        builder.Property(x => x.StartedAtUtc).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.TotalScore).HasPrecision(8, 2);

        builder.HasIndex(x => new { x.ExamId, x.StudentProfileId, x.AttemptNumber }).IsUnique();
    }
}