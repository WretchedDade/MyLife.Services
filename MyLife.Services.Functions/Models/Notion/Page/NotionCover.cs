using System.Text.Json.Serialization;

namespace MyLife.Services.Functions.Models.Notion.Page;
internal class NotionCover
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("external")]
    public NotionExternalCover External { get; set; }
}
