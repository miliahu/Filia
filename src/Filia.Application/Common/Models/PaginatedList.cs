namespace Filia.Application.Common.Models;

/// <summary>
/// Plain paging envelope. Deliberately has no knowledge of EF Core or any
/// other data-access technology - the repository does the actual paging
/// query and hands back the items + total count, which handlers wrap in
/// this type for the API to return.
/// </summary>
public class PaginatedList<T>(IReadOnlyCollection<T> items, int count, int pageNumber, int pageSize)
{
    public IReadOnlyCollection<T> Items { get; } = items;
    public int PageNumber { get; } = pageNumber;
    public int TotalPages { get; } = pageSize <= 0 ? 0 : (int)Math.Ceiling(count / (double)pageSize);
    public int TotalCount { get; } = count;

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
