namespace Template.Common;

public class PaginationFilter
{
    private const int MaxPageSize = 100;

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public int GetSafeSkip()
    {
        var pageNumber = PageNumber < 1 ? 1 : PageNumber;
        var pageSize = PageSize < 1 ? 20 : Math.Min(PageSize, MaxPageSize);
        return (pageNumber - 1) * pageSize;
    }

    public int GetSafeTake()
    {
        return PageSize < 1 ? 20 : Math.Min(PageSize, MaxPageSize);
    }
}

