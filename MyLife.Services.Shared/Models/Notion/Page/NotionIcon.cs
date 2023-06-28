using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Models.Notion.Page;

public class NotionIcon
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("emoji")]
    public string? Emoji { get; set; }

    [JsonPropertyName("external")]
    public NotionExternalIcon? External { get; set; }
}

public class NotionExternalIcon
{
    [JsonPropertyName("url")]
    public string Url { get; set; }
}