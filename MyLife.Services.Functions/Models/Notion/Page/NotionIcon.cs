using System.Text.Json.Serialization;

namespace MyLife.Services.Functions.Models.Notion.Page;

internal class NotionIcon
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("emoji")]
    public string? Emoji { get; set; }

    [JsonPropertyName("external")]
    public NotionExternalIcon? External { get; set; }
}

internal class NotionExternalIcon
{
    [JsonPropertyName("url")]
    public string Url { get; set; }
}