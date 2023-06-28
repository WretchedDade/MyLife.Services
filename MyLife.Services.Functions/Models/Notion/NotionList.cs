using MyLife.Services.Functions.Models.Notion.Page;
using System;
using System.Text.Json.Serialization;

namespace MyLife.Services.Functions.Models.Notion;
internal class NotionList<TResult> : NotionObject where TResult : NotionPage
{
    [JsonPropertyName("Results")]
    public TResult[] Results { get; set; } = Array.Empty<TResult>();

    [JsonPropertyName("next_cursor")]
    public string? NextCursor { get; set; }

    [JsonPropertyName("has_more")]
    public bool HasMore { get; set; }
}
