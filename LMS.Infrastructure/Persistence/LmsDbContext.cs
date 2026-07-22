using Application.Abstractions.Persistence;
using Domain.Entities.Academics;
using Domain.Entities.Users;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Exams;
namespace Infrastructure.Persistence;

public sealed class LmsDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
{
    public LmsDbContext(DbContextOptions<LmsDbContext> options) : base(options)
    {
    }

    public IQueryable<Faculty> Faculties => Set<Faculty>().AsQueryable();
    public IQueryable<Major> Majors => Set<Major>().AsQueryable();

    public IQueryable<UserProfile> UserProfiles => Set<UserProfile>().AsQueryable();
    public IQueryable<StudentProfile> StudentProfiles => Set<StudentProfile>().AsQueryable();
    public IQueryable<InstructorProfile> InstructorProfiles => Set<InstructorProfile>().AsQueryable();
    public IQueryable<EducationExpertProfile> EducationExpertProfiles => Set<EducationExpertProfile>().AsQueryable();
    public IQueryable<University> Universities => Set<University>().AsQueryable();
    public IQueryable<Course> Courses => Set<Course>().AsQueryable();
    public IQueryable<Semester> Semesters => Set<Semester>().AsQueryable();
    public IQueryable<CourseOffering> CourseOfferings => Set<CourseOffering>().AsQueryable();
    public IQueryable<Enrollment> Enrollments => Set<Enrollment>().AsQueryable();
    public IQueryable<QuestionBank> QuestionBanks => Set<QuestionBank>().AsQueryable();
    public IQueryable<Question> Questions => Set<Question>().AsQueryable();
    public IQueryable<QuestionOption> QuestionOptions => Set<QuestionOption>().AsQueryable();
    public IQueryable<Exam> Exams => Set<Exam>().AsQueryable();
    public IQueryable<ExamQuestion> ExamQuestions => Set<ExamQuestion>().AsQueryable();
    public IQueryable<ExamSubmission> ExamSubmissions => Set<ExamSubmission>().AsQueryable();
    public IQueryable<ExamAnswer> ExamAnswers => Set<ExamAnswer>().AsQueryable();
    public Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : class
        => Set<TEntity>().AddAsync(entity, cancellationToken).AsTask();

    public new Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => base.SaveChangesAsync(cancellationToken);

    public async Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var tx = await Database.BeginTransactionAsync(cancellationToken);
        return new EfAppTransaction(tx);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(LmsDbContext).Assembly);
    }
}