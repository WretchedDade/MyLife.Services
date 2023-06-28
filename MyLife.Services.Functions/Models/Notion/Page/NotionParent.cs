using System.Text.Json.Serialization;

namespace MyLife.Services.Functions.Models.Notion.Page;

internal class NotionParent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "database_id";

    [JsonPropertyName("database_id")]
    public string DatabaseId { get; set; }

}
