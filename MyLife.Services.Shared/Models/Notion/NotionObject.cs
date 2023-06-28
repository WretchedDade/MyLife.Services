using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Models.Notion;

public class NotionObject
{
    [JsonPropertyName("object")]
    public string Object { get; set; }
}
