namespace Application.Common.Models;

public abstract record PagedQuery
{
    public string? Search { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public string? SortBy { get; init; }

    public bool SortDescending { get; init; }
}