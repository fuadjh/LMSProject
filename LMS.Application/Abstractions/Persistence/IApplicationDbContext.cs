using Domain.Entities.Academics;
using Domain.Entities.Users;

namespace Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    IQueryable<University> Universities { get; }

    IQueryable<Faculty> Faculties { get; }

    IQueryable<Major> Majors { get; }

    IQueryable<UserProfile> UserProfiles { get; }

    IQueryable<StudentProfile> StudentProfiles { get; }

    IQueryable<InstructorProfile> InstructorProfiles { get; }

    IQueryable<ExpertProfile> ExpertProfiles { get; }

    Task AddAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default)
        where TEntity : class;

    void Remove<TEntity>(TEntity entity)
        where TEntity : class;

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task<IAppTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default);
}