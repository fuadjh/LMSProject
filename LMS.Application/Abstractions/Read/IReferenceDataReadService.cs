using Application.Common.Models;

using Common.Contracts.ReferenceData;


namespace Application.Abstractions.Read;

public interface IReferenceDataReadService
{
    Task<IReadOnlyCollection<LookupItemDto>> GetFacultiesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<LookupItemDto>> GetMajorsAsync(
        Guid? facultyId,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<UniversityListItemDto>> GetUniversitiesAsync(
        PagedQuery query,
        CancellationToken cancellationToken = default);
}