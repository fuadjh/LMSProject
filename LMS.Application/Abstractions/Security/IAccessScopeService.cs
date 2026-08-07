namespace Application.Abstractions.Security;

public interface IAccessScopeService
{
    Task<bool> HasFacultyAccessAsync(Guid UserId, Guid facultyId, CancellationToken cancellationToken = default);
    Task<bool> HasMajorAccessAsync(Guid UserId, Guid majorId, CancellationToken cancellationToken = default);
}