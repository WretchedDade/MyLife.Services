using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Models.Notion.Page;

public class NotionParent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "database_id";

    [JsonPropertyName("database_id")]
    public string DatabaseId { get; set; }

}
