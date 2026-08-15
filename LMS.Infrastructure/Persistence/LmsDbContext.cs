using Application.Abstractions.Persistence;
using Domain.Entities.Academics;
using Domain.Entities.Exams;
using Domain.Entities.Learning;
using Domain.Entities.Users;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class LmsDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>,
      IApplicationDbContext
{
    public LmsDbContext()
    {
    }

    public LmsDbContext(
        DbContextOptions<LmsDbContext> options)
        : base(options)
    {
    }

    public IQueryable<University> Universities =>
        Set<University>();

    public IQueryable<Faculty> Faculties =>
        Set<Faculty>();

    public IQueryable<Major> Majors =>
        Set<Major>();

    public IQueryable<Course> Courses =>
        Set<Course>();

    public IQueryable<Semester> Semesters =>
        Set<Semester>();

    public IQueryable<CourseOffering> CourseOfferings =>
        Set<CourseOffering>();

    public IQueryable<Enrollment> Enrollments =>
        Set<Enrollment>();

    public IQueryable<QuestionBank> QuestionBanks =>
        Set<QuestionBank>();

    public IQueryable<Question> Questions =>
        Set<Question>();

    public IQueryable<QuestionOption> QuestionOptions =>
        Set<QuestionOption>();

    public IQueryable<Exam> Exams =>
        Set<Exam>();

    public IQueryable<ExamQuestion> ExamQuestions =>
        Set<ExamQuestion>();

    public IQueryable<ExamSubmission> ExamSubmissions =>
        Set<ExamSubmission>();

    public IQueryable<ExamAnswer> ExamAnswers =>
        Set<ExamAnswer>();

    public IQueryable<LearningModule> LearningModules =>
        Set<LearningModule>();

    public IQueryable<LearningItem> LearningItems =>
        Set<LearningItem>();

    public IQueryable<LearningItemProgress> LearningItemProgresses =>
        Set<LearningItemProgress>();

    public IQueryable<LearningTemplate> LearningTemplates =>
        Set<LearningTemplate>();

    public IQueryable<LearningTemplateModule> LearningTemplateModules =>
        Set<LearningTemplateModule>();

    public IQueryable<LearningTemplateItem> LearningTemplateItems =>
        Set<LearningTemplateItem>();

    public IQueryable<UserProfile> UserProfiles =>
      Set<UserProfile>();

    public IQueryable<StudentProfile> StudentProfiles =>
        Set<StudentProfile>();

    public IQueryable<InstructorProfile> InstructorProfiles =>
        Set<InstructorProfile>();

    public IQueryable<ExpertProfile> ExpertProfiles =>
        Set<ExpertProfile>();


    public Task AddAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default)
        where TEntity : class =>
        Set<TEntity>()
            .AddAsync(entity, cancellationToken)
            .AsTask();

    public void Remove<TEntity>(TEntity entity)
        where TEntity : class =>
        Set<TEntity>().Remove(entity);

    public IQueryable<UserFacultyScope> UserFacultyScopes =>
        Set<UserFacultyScope>();

    public IQueryable<UserMajorScope> UserMajorScopes =>
        Set<UserMajorScope>();

   

   
    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    public async Task<IAppTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        var transaction =
            await Database.BeginTransactionAsync(cancellationToken);

        return new EfAppTransaction(transaction);
    }

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        var connectionString =
            Environment.GetEnvironmentVariable(
                "ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString =
                "Server=(localdb)\\MSSQLLocalDB;" +
                "Database=LmsDb;" +
                "Trusted_Connection=True;" +
                "TrustServerCertificate=True;" +
                "MultipleActiveResultSets=True";
        }

        optionsBuilder.UseSqlServer(
            connectionString,
            sqlServerOptions =>
            {
                sqlServerOptions.MigrationsAssembly(
                    typeof(LmsDbContext).Assembly.FullName);
            });
    }

    protected override void OnModelCreating(
        ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(
            typeof(LmsDbContext).Assembly);
    }
}