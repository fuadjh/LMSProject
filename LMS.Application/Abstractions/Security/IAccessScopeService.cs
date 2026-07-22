namespace Application.Abstractions.Security;

public interface IAccessScopeService
{
    Task<bool> HasFacultyAccessAsync(Guid userProfileId, Guid facultyId, CancellationToken cancellationToken = default);
    Task<bool> HasMajorAccessAsync(Guid userProfileId, Guid majorId, CancellationToken cancellationToken = default);
}