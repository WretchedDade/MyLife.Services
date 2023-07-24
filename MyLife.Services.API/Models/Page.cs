namespace MyLife.Services.API.Models;

public class Page<TModel> where TModel : class
{
    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages => PageSize == 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public List<TModel> Items { get; set; } = new();

    public bool HasPreviousPage => PageNumber >= 1;

    public bool HasNextPage => PageNumber < (TotalPages - 1);

    public Page(int pageNumber, int? pageSize, int totalCount, List<TModel> items)
    {
        PageNumber = pageNumber;
        PageSize = pageSize ?? items.Count;
        TotalCount = totalCount;
        Items = items;
    }
}