namespace mini_mes_be.DTOs.Pagination;

/// <summary>
/// Common pagination + search + sort request parameters.
/// Used as query parameters on all list endpoints.
/// </summary>
public class PaginatedRequest
{
    public int page { get; set; } = 1;
    public int page_size { get; set; } = 20;
    public string? search { get; set; }
    public string? sort_by { get; set; }
    public string sort_direction { get; set; } = "asc";
}

/// <summary>
/// Wraps a page of items with pagination metadata.
/// </summary>
public class PaginatedResponse<T>
{
    public IEnumerable<T> items { get; set; } = [];
    public int total_count { get; set; }
    public int page { get; set; }
    public int page_size { get; set; }
    public int total_pages => (int)Math.Ceiling((double)total_count / page_size);
    public bool has_next => page < total_pages;
    public bool has_previous => page > 1;

    public static PaginatedResponse<T> Create(IEnumerable<T> items, int totalCount, int page, int pageSize) =>
        new()
        {
            items = items,
            total_count = totalCount,
            page = page,
            page_size = pageSize
        };
}
