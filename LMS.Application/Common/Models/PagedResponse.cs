namespace Application.Common.Models;

public sealed record PagedResponse<T>(
    IReadOnlyCollection<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize)
{
    public int TotalPages =>
        PageSize <= 0
            ? 0
            : (int)Math.Ceiling(TotalCount / (double)PageSize);
}