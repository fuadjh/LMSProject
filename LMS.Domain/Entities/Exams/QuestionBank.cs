using Domain.Common;

namespace Domain.Entities.Exams;

public sealed class QuestionBank : BaseEntity
{
    private QuestionBank() { }

    public Guid CourseId { get; private set; }
    public Guid InstructorProfileId { get; private set; }
    public bool IsActive { get; private set; }

    public static QuestionBank Create(Guid courseId, Guid instructorProfileId)
    {
        if (courseId == Guid.Empty)
            throw new ArgumentException("CourseId is required.");

        if (instructorProfileId == Guid.Empty)
            throw new ArgumentException("InstructorProfileId is required.");

        return new QuestionBank
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            InstructorProfileId = instructorProfileId,
            IsActive = true
        };
    }
}