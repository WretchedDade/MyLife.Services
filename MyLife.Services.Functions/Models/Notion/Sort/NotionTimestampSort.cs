using System;
using System.Text.Json.Serialization;

namespace MyLife.Services.Functions.Models.Notion.Sort;

internal sealed class NotionTimestampSort : NotionSort
{
    public NotionTimestampSort()
    {

    }

    public NotionTimestampSort(NotionTimestampTypes timestampType, NotionSortDirections sortDirection) : base(sortDirection)
    {
        Timestamp = timestampType switch
        {
            NotionTimestampTypes.CreatedTime => "created_time",
            NotionTimestampTypes.LastEditedTime => "last_edited_time",
            _ => throw new NotImplementedException(),
        };
    }

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;
}
