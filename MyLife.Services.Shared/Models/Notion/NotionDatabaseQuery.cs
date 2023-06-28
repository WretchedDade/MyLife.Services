using MyLife.Services.Shared.Models.Notion.Filter;
using MyLife.Services.Shared.Models.Notion.Sort;
using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Models.Notion;
public class NotionDatabaseQuery
{
    [JsonPropertyName("start_cursor")]
    public string? StartCursor { get; set; }

    [JsonPropertyName("page_size")]
    public int PageSize { get; set; } = 10;

    [JsonPropertyName("filter")]
    public NotionFilter? Filter { get; set; }

    [JsonPropertyName("sorts")]
    public NotionSort[]? Sorts { get; set; }
}
