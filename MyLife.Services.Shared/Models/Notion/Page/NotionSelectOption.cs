using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Models.Notion.Page;
public class NotionSelectOption
{
    public NotionSelectOption(string name)
    {
        Name = name;
    }

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}
