using System.Text.Json.Serialization;

namespace MyLife.Services.Functions.Models.Notion.Page;

internal class NotionText
{
    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonPropertyName("link")]
    public string? Link { get; set; }
}
