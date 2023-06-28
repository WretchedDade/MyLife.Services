using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Models.Notion.Page;
public class NotionCover
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("external")]
    public NotionExternalCover External { get; set; }
}
