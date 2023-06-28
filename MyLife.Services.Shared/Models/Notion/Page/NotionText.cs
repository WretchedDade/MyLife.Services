using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Models.Notion.Page;

public class NotionText
{
    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonPropertyName("link")]
    public string? Link { get; set; }
}
