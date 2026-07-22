using Application.Common.Models;
using Common.Contracts.Academic;

namespace Application.Abstractions.Read;

public interface IAcademicReadService
{
    Task<IReadOnlyCollection<CourseLookupDto>> GetCoursesAsync(Guid? majorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<SemesterLookupDto>> GetSemestersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CourseOfferingLookupDto>> GetCourseOfferingsAsync(Guid? semesterId, CancellationToken cancellationToken = default);
    Task<CourseOfferingDetailsDto?> GetCourseOfferingAsync(Guid offeringId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InstructorLookupDto>> SearchInstructorsAsync(string? search, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<StudentLookupDto>> SearchStudentsAsync(Guid? majorId, string? search, CancellationToken cancellationToken = default);
}