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
    : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        Guid>,
      IApplicationDbContext
{
    /// <summary>
    /// فقط برای ابزارهای Design-Time مربوط به EF Core استفاده می‌شود.
    /// در اجرای معمول برنامه، Constructor دارای DbContextOptions
    /// توسط Dependency Injection فراخوانی می‌شود.
    /// </summary>
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

    public IQueryable<LearningItemProgress>
        LearningItemProgresses =>
        Set<LearningItemProgress>();

    public IQueryable<LearningTemplate> LearningTemplates =>
        Set<LearningTemplate>();

    public IQueryable<LearningTemplateModule>
        LearningTemplateModules =>
        Set<LearningTemplateModule>();

    public IQueryable<LearningTemplateItem>
        LearningTemplateItems =>
        Set<LearningTemplateItem>();

    public IQueryable<UserProfile> UserProfiles =>
        Set<UserProfile>();

    public IQueryable<StudentProfile> StudentProfiles =>
        Set<StudentProfile>();

    public IQueryable<InstructorProfile> InstructorProfiles =>
        Set<InstructorProfile>();

    public IQueryable<EducationExpertProfile>
        EducationExpertProfiles =>
        Set<EducationExpertProfile>();

    public Task AddAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default)
        where TEntity : class =>
        Set<TEntity>()
            .AddAsync(entity, cancellationToken)
            .AsTask();

    public new Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default) =>
        base.SaveChangesAsync(cancellationToken);

    public async Task<IAppTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        var transaction =
            await Database.BeginTransactionAsync(
                cancellationToken);

        return new EfAppTransaction(transaction);
    }

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        // هنگام اجرای عادی برنامه، تنظیمات توسط AddDbContext
        // در WebApi اعمال شده‌اند و نباید بازنویسی شوند.
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        // قابل تنظیم برای Migration و محیط‌های CI/CD.
        var connectionString =
            Environment.GetEnvironmentVariable(
                "ConnectionStrings__DefaultConnection");

        // مقدار جایگزین فقط برای Design-Time و محیط توسعه.
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