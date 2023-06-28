using System.Text.Json.Serialization;

namespace MyLife.Services.Functions.Models.Notion;

internal class NotionObject
{
    [JsonPropertyName("object")]
    public string Object { get; set; }
}
