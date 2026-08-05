using Domain.Entities.Academics;
using Domain.Entities.Exams;
using Domain.Entities.Learning;
using Domain.Entities.Users;

namespace Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    IQueryable<University> Universities { get; }
    IQueryable<Faculty> Faculties { get; }
    IQueryable<Major> Majors { get; }

    IQueryable<Course> Courses { get; }
    IQueryable<Semester> Semesters { get; }
    IQueryable<CourseOffering> CourseOfferings { get; }
    IQueryable<Enrollment> Enrollments { get; }

    IQueryable<QuestionBank> QuestionBanks { get; }
    IQueryable<Question> Questions { get; }
    IQueryable<QuestionOption> QuestionOptions { get; }
    IQueryable<Exam> Exams { get; }
    IQueryable<ExamQuestion> ExamQuestions { get; }
    IQueryable<ExamSubmission> ExamSubmissions { get; }
    IQueryable<ExamAnswer> ExamAnswers { get; }

    IQueryable<LearningModule> LearningModules { get; }
    IQueryable<LearningItem> LearningItems { get; }
    IQueryable<LearningItemProgress> LearningItemProgresses { get; }
    IQueryable<LearningTemplate> LearningTemplates { get; }
    IQueryable<LearningTemplateModule> LearningTemplateModules { get; }
    IQueryable<LearningTemplateItem> LearningTemplateItems { get; }

    IQueryable<UserProfile> UserProfiles { get; }
    IQueryable<StudentProfile> StudentProfiles { get; }
    IQueryable<InstructorProfile> InstructorProfiles { get; }
    IQueryable<EducationExpertProfile> EducationExpertProfiles { get; }
    void Remove<TEntity>(TEntity entity)
    where TEntity : class;
    Task AddAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default)
        where TEntity : class;

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task<IAppTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default);
}