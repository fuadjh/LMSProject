using Domain.Entities.Exams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class QuestionBankConfiguration : IEntityTypeConfiguration<QuestionBank>
{
    public void Configure(EntityTypeBuilder<QuestionBank> builder)
    {
        builder.ToTable("QuestionBanks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CourseId).IsRequired();
        builder.Property(x => x.InstructorProfileId).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();

        builder.HasIndex(x => new { x.CourseId, x.InstructorProfileId }).IsUnique();
    }
}