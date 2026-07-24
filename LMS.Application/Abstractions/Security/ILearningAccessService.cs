namespace Application.Abstractions.Security;

public interface ILearningAccessService
{
    Task<bool> CanManageOfferingAsync(
        Guid courseOfferingId,
        CancellationToken cancellationToken = default);

    Task<bool> CanViewOfferingAsync(
        Guid courseOfferingId,
        CancellationToken cancellationToken = default);

    Task<Guid?> GetCurrentStudentProfileIdAsync(
        CancellationToken cancellationToken = default);

    Task<Guid?> GetCurrentInstructorProfileIdAsync(
        CancellationToken cancellationToken = default);
}